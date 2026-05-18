using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;
using EstacionamentoAPI.Services.Interfaces;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Services
{
    public class AcessoService
        : IAcessoService
    {
        private readonly
            IAcessoRepository
            _acessoRepository;

        private readonly
            ITarifaService
            _tarifaService;

        private readonly
            EstacionamentoContext
            _context;

        public AcessoService(
            IAcessoRepository acessoRepository,
            ITarifaService tarifaService,
            EstacionamentoContext context)
        {
            _acessoRepository =
                acessoRepository;

            _tarifaService =
                tarifaService;

            _context = context;
        }

        public async Task<IActionResult>
            RegistrarEntrada(
                DadosEntrada dados)
        {
            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                bool veiculoNoPatio =
                    await _acessoRepository
                        .ExisteVeiculoNoPatio(
                            dados.Placa);

                if (veiculoNoPatio)
                {
                    return new BadRequestObjectResult(
                        $"O veículo {dados.Placa} já está no estacionamento.");
                }

                var vaga =
                    await _context.Vagas
                    .FirstOrDefaultAsync(v =>
                        v.Status == "Disponivel"
                        &&
                        v.TipoVaga.ToLower()
                        ==
                        dados.TipoVeiculo
                        .ToLower());

                if (vaga == null)
                {
                    return new BadRequestObjectResult($"Não há vagas disponíveis para {dados.TipoVeiculo}");
                }

                // Lock pessimista
                vaga =
                    await _acessoRepository
                        .ObterVagaComLock(vaga.IdVaga);

                if (vaga == null)
                {
                    return new NotFoundObjectResult("Vaga não encontrada.");
                }

                // INTEGRIDADE TEMPORAL
                bool vagaOcupada =
                    await _acessoRepository
                        .ExisteOcupacaoAtivaNaVaga(vaga.IdVaga);

                if (vagaOcupada)
                {
                    return new ConflictObjectResult(
                        new
                        {
                            mensagem =
                            "A vaga já possui ocupação ativa."
                        });
                }

                vaga.Status =
                    "Ocupada";

                var acesso =
                    new AcessoVeiculo
                    {
                        Placa =
                            dados.Placa,

                        IdVaga =
                            vaga.IdVaga,

                        HoraEntrada =
                            DateTime.UtcNow,

                        StatusPagamento =
                            "Pendente"
                    };

                await _acessoRepository
                    .AdicionarAcesso(
                        acesso);

                await _acessoRepository
                    .SaveChanges();

                await transaction
                    .CommitAsync();

                return new OkObjectResult(
                    acesso);
            }
            catch
            {
                await transaction
                    .RollbackAsync();

                return new ConflictObjectResult(
                    new
                    {
                        mensagem = "Erro de concorrência na entrada."
                    });
            }
        }

        public async Task<IActionResult>
            RegistrarSaida(
                int idAcesso,
                DadosSaida dados)
       {
            await using var transaction =
                await _context.Database
                .BeginTransactionAsync();

            try
            {
                var acesso =
                    await _acessoRepository
                    .GetById(idAcesso);

                if (acesso == null)
                {
                    return new NotFoundObjectResult("Ticket não encontrado.");
                }

                if (acesso.HoraSaida != null)
                {
                    return new BadRequestObjectResult("Saída já registrada.");
                }

                acesso.HoraSaida = DateTime.UtcNow;

               // cálculo permanência
               TimeSpan permanencia = acesso.HoraSaida.Value - acesso.HoraEntrada;

               // chama TarifaService
               var resultadoTarifa =
                await _tarifaService
                .CalcularTarifaAsync(acesso.Placa, permanencia);

               decimal valorFinal = resultadoTarifa.valorFinal;

               string tipoAplicado = resultadoTarifa.tipoAplicado;

               // atualização pagamento
               acesso.ValorPago = valorFinal;

               acesso.StatusPagamento = "Concluido";

               acesso.FormaPagamento = dados.FormaPagamento;

               // busca vaga
               var vaga =
                await _acessoRepository
                .GetVagaById(acesso.IdVaga);

               if (vaga != null)
               {
                vaga.Status = "Disponivel";
               }

              await _acessoRepository
                .SaveChanges();

              await transaction
                .CommitAsync();

              return new OkObjectResult( 
                new
                {
                   Placa = acesso.Placa,

                   TempoPermanencia = permanencia
                    .ToString(@"hh\:mm"),

                   ValorFinal = valorFinal,

                   FormaPagamento = acesso.FormaPagamento,

                   TipoTarifa = tipoAplicado,

                Mensagem = "Saída registrada com sucesso"
                });
            }
            catch (Exception ex)
            {
            await transaction
            .RollbackAsync();

            return new ObjectResult(
            new
            {
                mensagem = "Erro ao registrar saída.",

                detalhe = ex.Message
            })
           {
            StatusCode = 500
           };
           }
        }
    }
}