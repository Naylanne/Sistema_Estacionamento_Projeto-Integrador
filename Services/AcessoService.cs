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

        public AcessoService(IAcessoRepository acessoRepository, ITarifaService tarifaService,
            EstacionamentoContext context)
        {
            _acessoRepository = acessoRepository;
            _tarifaService = tarifaService;
            _context = context;
        }

        public async Task<IEnumerable<Acesso>> GetAcessos()
        {
            return await _context.Acessos.ToListAsync();
        }
        private string GerarTicket()
        {
            return $"TKT-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }

        public async Task<IActionResult> RegistrarEntrada(DadosEntrada DTOs)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Buscar veículo
                var veiculo = await _context.Veiculos
                    .FirstOrDefaultAsync(v =>
                        v.Placa == DTOs.Placa);

                if (veiculo == null)
                {
                    return new NotFoundObjectResult(
                        "Veículo não encontrado.");
                }

                // 2. Verificar se já está no pátio
                bool veiculoNoPatio =
                    await _acessoRepository
                        .ExisteVeiculoNoPatio(DTOs.Placa);

                if (veiculoNoPatio)
                {
                    return new BadRequestObjectResult(
                        $"O veículo {DTOs.Placa} Veículo já está no estacionamento.");
                }

                // 3. Buscar vaga disponível com Lock Pessimista para evitar condições de corrida
                var vaga =
                    await _acessoRepository
                        .ObterPrimeiraVagaDisponivelComLock(
                            veiculo.TipoVeiculo);

                if (vaga == null)
                {
                    return new BadRequestObjectResult(
                        $"Não há vagas disponíveis para {veiculo.TipoVeiculo}");
                }

                // 4. Marcar vaga como ocupada
                vaga.Status = "Ocupada";

                // Tarifa padrão (por enquanto)
                int idTarifa = 1;

                // 5. Criar acesso
                var acesso = new Acesso
                {
                    Ticket = GerarTicket(),
                    IdVeiculo = veiculo.IdVeiculo,
                    IdVaga = vaga.IdVaga,
                    IdTarifa = idTarifa,
                    HoraEntrada = DateTime.UtcNow
                };

                await _acessoRepository
                    .AdicionarAcesso(acesso);

                await _acessoRepository.SaveChanges();

                await transaction.CommitAsync();

                return new OkObjectResult(new
                {
                    acesso.IdAcesso,
                    acesso.Ticket,
                    veiculo.Placa,
                    vaga.IdVaga,
                    acesso.HoraEntrada,
                    Mensagem = "Entrada registrada com sucesso"
});
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new ObjectResult(new
                {
                    mensagem = "Erro ao registrar entrada.",
                    detalhe = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> RegistrarSaida(int idAcesso, DadosSaida DTOs)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Buscar acesso com Lock
                var acesso =
                    await _acessoRepository
                        .GetByIdComLock(idAcesso);

                if (acesso == null)
                {
                    return new NotFoundObjectResult(
                        "Ticket não encontrado.");
                }

                // 2. Verificar saída já registrada
                if (acesso.HoraSaida != null)
                {
                    return new BadRequestObjectResult(
                        "Saída já registrada.");
                }

                acesso.HoraSaida =
                    DateTime.UtcNow;

                // 3. Calcular permanência
                TimeSpan permanencia =
                    acesso.HoraSaida.Value
                    - acesso.HoraEntrada;

                acesso.TempoPermanencia =
                    permanencia;

                // 4. Buscar veículo
                var veiculo =
                    await _context.Veiculos
                        .FirstOrDefaultAsync(v =>
                            v.IdVeiculo ==
                            acesso.IdVeiculo);

                if (veiculo == null)
                {
                    return new NotFoundObjectResult("Veículo não encontrado.");
                }

                // 5. Calcular tarifa
                var resultadoTarifa =
                    await _tarifaService
                        .CalcularTarifaAsync(
                            veiculo.Placa,
                            permanencia);

                decimal valorFinal =
                    resultadoTarifa.valorFinal;

                string tipoAplicado =
                    resultadoTarifa.tipoAplicado;

                // 6. Criar pagamento (1:1)
                var pagamento = new Pagamento
                {
                    IdAcesso = acesso.IdAcesso,
                    DataHora = DateTime.UtcNow,
                    ValorPago = valorFinal,
                    FormaPagamento = DTOs.FormaPagamento,
                    StatusPagamento = "Concluido"
                };

                _context.Pagamentos.Add(
                    pagamento);

                // 7. Liberar vaga com Lock
                var vaga =
                    await _acessoRepository
                        .ObterVagaComLock(
                            acesso.IdVaga);

                if (vaga != null)
                {
                    vaga.Status = "Disponivel";
                }

                await _acessoRepository
                    .SaveChanges();

                await transaction.CommitAsync();

                return new OkObjectResult(new
                {
                    veiculo.Placa,
                    TempoPermanencia =
                        permanencia.ToString(
                            @"hh\:mm"),

                    ValorFinal = valorFinal,

                    FormaPagamento = DTOs.FormaPagamento,

                    TipoTarifa = tipoAplicado,

                    Mensagem = "Saída registrada com sucesso"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new ObjectResult(new
                {
                    mensagem ="Erro ao registrar saída.",
                    detalhe = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }
    }
}