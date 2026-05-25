using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;
using EstacionamentoAPI.Services.Interfaces;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Services
{
    public class AcessoService : IAcessoService
    {
        private readonly IAcessoRepository _acessoRepository;
        private readonly ITarifaService _tarifaService;
        private readonly EstacionamentoContext _context;

        public AcessoService(
            IAcessoRepository acessoRepository,
            ITarifaService tarifaService,
            EstacionamentoContext context)
        {
            _acessoRepository = acessoRepository;
            _tarifaService = tarifaService;
            _context = context;
        }

        public async Task<IActionResult> RegistrarEntrada(DadosEntrada dados)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validar se veículo já está no pátio
                bool veiculoNoPatio = await _acessoRepository.ExisteVeiculoNoPatio(dados.Placa);
                if (veiculoNoPatio)
                {
                    return new BadRequestObjectResult($"O veículo {dados.Placa} já está no estacionamento.");
                }

                // Buscar a vaga disponível já aplicando o lock
                var vaga = await _acessoRepository.ObterPrimeiraVagaDisponivelComLock(dados.TipoVeiculo);

                if (vaga == null)
                {
                    return new BadRequestObjectResult($"Não há vagas disponíveis para {dados.TipoVeiculo}");
                }

                // Nenhuma outra requisição pegará a mesma vaga
                vaga.Status = "Ocupada";

                var acesso = new AcessoVeiculo
                {
                    Placa = dados.Placa,
                    IdVaga = vaga.IdVaga,
                    HoraEntrada = DateTime.UtcNow,
                    StatusPagamento = "Pendente"
                };

                await _acessoRepository.AdicionarAcesso(acesso);
                await _acessoRepository.SaveChanges();
                await transaction.CommitAsync();

                return new OkObjectResult(acesso);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ObjectResult(new { mensagem = "Erro ao registrar entrada.", detalhe = ex.Message }) 
                { 
                    StatusCode = 500 
                };
            }
        }

        public async Task<IActionResult> RegistrarSaida(int idAcesso, DadosSaida dados)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Busca o ticket de acesso aplicando lock pessimista
                var acesso = await _acessoRepository.GetByIdComLock(idAcesso);

                if (acesso == null)
                {
                    return new NotFoundObjectResult("Ticket não encontrado.");
                }

                if (acesso.HoraSaida != null)
                {
                    return new BadRequestObjectResult("Saída já registrada.");
                }

                acesso.HoraSaida = DateTime.UtcNow;

                // Cálculo permanência
                TimeSpan permanencia = acesso.HoraSaida.Value - acesso.HoraEntrada;

                // Chama TarifaService
                var resultadoTarifa = await _tarifaService.CalcularTarifaAsync(acesso.Placa, permanencia);
                decimal valorFinal = resultadoTarifa.valorFinal;
                string tipoAplicado = resultadoTarifa.tipoAplicado;

                // Atualização pagamento
                acesso.ValorPago = valorFinal;
                acesso.StatusPagamento = "Concluido";
                acesso.FormaPagamento = dados.FormaPagamento;

                // Busca a vaga associada também aplicando lock para garantir a consistência de estados
                var vaga = await _acessoRepository.ObterVagaComLock(acesso.IdVaga);
                if (vaga != null)
                {
                    vaga.Status = "Disponivel";
                }

                await _acessoRepository.SaveChanges();
                await transaction.CommitAsync();

                return new OkObjectResult(new
                {
                    Placa = acesso.Placa,
                    TempoPermanencia = permanencia.ToString(@"hh\:mm"),
                    ValorFinal = valorFinal,
                    FormaPagamento = acesso.FormaPagamento,
                    TipoTarifa = tipoAplicado,
                    Mensagem = "Saída registrada com sucesso"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ObjectResult(new { mensagem = "Erro ao registrar saída.", detalhe = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }
    }
}