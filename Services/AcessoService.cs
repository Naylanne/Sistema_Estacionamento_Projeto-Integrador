using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;
using EstacionamentoAPI.Services.Interfaces;
using EstacionamentoAPI.DTOs;
using EstacionamentoAPI.Enums;

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

        public async Task<IEnumerable<Acesso>> GetAcessos()
        {
            return await _context.Acessos.ToListAsync();
        }

        public async Task<IEnumerable<Acesso>> GetAcessosAtivos()
        {
            return await _context.Acessos
                .Where(a => a.HoraSaida == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Acesso>> GetAcessosFinalizados()
        {
            return await _context.Acessos
                .Where(a => a.HoraSaida != null)
                .ToListAsync();
        }

        private string GerarTicket()
        {
            return $"TKT-{DateTime.Now:yyyyMMddHHmmssfff}";
        }

        private static PostgresException? ObterErroPostgres(Exception ex)
        {
            Exception? excecaoAtual = ex;

            while (excecaoAtual != null)
            {
                if (excecaoAtual is PostgresException postgresException)
                {
                    return postgresException;
                }

                excecaoAtual = excecaoAtual.InnerException;
            }

            return null;
        }

        public async Task<IActionResult> RegistrarEntrada(DadosEntrada DTOs)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var placa = DTOs.Placa.Trim().ToUpper();

                // 1. Buscar veículo com LOCK PESSIMISTA
                // O FOR UPDATE trava a linha do veículo durante a transação.
                // Se duas requisições tentarem registrar entrada para a mesma placa,
                // a segunda aguarda a primeira finalizar.
                var veiculo = await _context.Veiculos
                    .FromSqlInterpolated($@"
                        SELECT *
                        FROM veiculo
                        WHERE placa = {placa}
                        FOR UPDATE
                    ")
                    .FirstOrDefaultAsync();

                if (veiculo == null)
                {
                    await transaction.RollbackAsync();

                    return new NotFoundObjectResult(
                        "Veículo não encontrado.");
                }

                // 2. Verificar se já está no pátio
                // Essa verificação acontece depois do lock pessimista.
                bool veiculoNoPatio = await _context.Acessos
                    .AnyAsync(a =>
                        a.IdVeiculo == veiculo.IdVeiculo &&
                        a.HoraSaida == null);

                if (veiculoNoPatio)
                {
                    await transaction.RollbackAsync();

                    return new BadRequestObjectResult(new
                    {
                        mensagem = $"O veículo {placa} já está no estacionamento."
                    });
                }

                // 3. Buscar vaga disponível com lock pessimista
                var vaga = await _acessoRepository
                    .ObterPrimeiraVagaDisponivelComLock(
                        veiculo.TipoVeiculo);

                if (vaga == null)
                {
                    await transaction.RollbackAsync();

                    return new BadRequestObjectResult(new
                    {
                        mensagem = $"Não há vagas disponíveis para {veiculo.TipoVeiculo}."
                    });
                }

                // 4. Marcar vaga como ocupada
                vaga.Status = "Ocupada";
                _context.Vagas.Update(vaga);

                // Tarifa padrão
                int idTarifa = 1;

                // 5. Criar acesso
                var acesso = new Acesso
                {
                    Ticket = GerarTicket(),
                    IdVeiculo = veiculo.IdVeiculo,
                    IdVaga = vaga.IdVaga,
                    IdTarifa = idTarifa,
                    HoraEntrada = DateTime.Now
                };

                await _acessoRepository.AdicionarAcesso(acesso);

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
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                var erroPostgres = ObterErroPostgres(ex);

                // 23505 = violação de índice único no PostgreSQL.
                // Esse tratamento cobre a regra:
                // ix_acesso_veiculo_ativo_unico
                if (erroPostgres != null && erroPostgres.SqlState == "23505")
                {
                    return new BadRequestObjectResult(new
                    {
                        mensagem = "Entrada bloqueada: já existe um acesso ativo para este veículo."
                    });
                }

                return new ObjectResult(new
                {
                    mensagem = "Erro ao registrar entrada.",
                    detalhe = ex.GetBaseException().Message
                })
                {
                    StatusCode = 400
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new ObjectResult(new
                {
                    mensagem = "Erro ao registrar entrada.",
                    detalhe = ex.GetBaseException().Message
                })
                {
                    StatusCode = 400
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
                var acesso = await _acessoRepository
                    .GetByIdComLock(idAcesso);

                if (acesso == null)
                {
                    await transaction.RollbackAsync();

                    return new NotFoundObjectResult(
                        "Ticket não encontrado.");
                }

                // 2. Verificar saída já registrada
                if (acesso.HoraSaida != null)
                {
                    await transaction.RollbackAsync();

                    return new BadRequestObjectResult(
                        "Saída já registrada.");
                }

                acesso.HoraSaida = DateTime.Now;

                // 3. Calcular permanência
                TimeSpan permanencia =
                    acesso.HoraSaida.Value - acesso.HoraEntrada;

                acesso.TempoPermanencia = permanencia;

                // 4. Buscar veículo
                var veiculo = await _context.Veiculos
                    .FirstOrDefaultAsync(v =>
                        v.IdVeiculo == acesso.IdVeiculo);

                if (veiculo == null)
                {
                    await transaction.RollbackAsync();

                    return new NotFoundObjectResult(
                        "Veículo não encontrado.");
                }

                // 5. Calcular tarifa
                var resultadoTarifa = await _tarifaService
                    .CalcularTarifaAsync(
                        veiculo.Placa,
                        permanencia);

                decimal valorFinal =
                    resultadoTarifa.valorFinal;

                string tipoAplicado =
                    resultadoTarifa.tipoAplicado;

                // 6. Criar pagamento
                var pagamento = new Pagamento
                {
                    IdAcesso = acesso.IdAcesso,
                    DataHora = DateTime.Now,
                    ValorPago = valorFinal,
                    FormaPagamento = DTOs.FormaPagamento,
                    StatusPagamento = StatusPagamentoEnum.Concluido
                };

                _context.Pagamentos.Add(pagamento);

                _context.Acessos.Update(acesso);

                // 7. Liberar vaga com Lock
                var vaga = await _acessoRepository
                    .ObterVagaComLock(acesso.IdVaga);

                if (vaga != null)
                {
                    vaga.Status = "Disponivel";
                    _context.Vagas.Update(vaga);
                }

                await _acessoRepository.SaveChanges();

                await transaction.CommitAsync();

                return new OkObjectResult(new
                {
                    veiculo.Placa,

                    TempoPermanencia =
                        permanencia.ToString(@"hh\:mm"),

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
                    mensagem = "Erro ao registrar saída.",
                    detalhe = ex.GetBaseException().Message
                })
                {
                    StatusCode = 400
                };
            }
        }

        public async Task<IEnumerable<HistoricoComPagamentoDto>> GetHistoricoComPagamento()
        {
            var query =
                from av in _context.Acessos
                join p in _context.Pagamentos
                    on av.IdAcesso equals p.IdAcesso into pagamentos
                from pagamento in pagamentos.DefaultIfEmpty()
                select new HistoricoComPagamentoDto
                {
                    IdAcesso = av.IdAcesso,
                    Ticket = av.Ticket,
                    HoraEntrada = av.HoraEntrada,
                    HoraSaida = av.HoraSaida,
                    TempoPermanencia = av.TempoPermanencia,
                    ValorPago = pagamento != null ? pagamento.ValorPago : null,
                    FormaPagamento = pagamento != null ? pagamento.FormaPagamento : null,
                    StatusPagamento = pagamento != null
                        ? pagamento.StatusPagamento.ToString()
                        : null,
                    DataPagamento = pagamento != null
                        ? pagamento.DataHora
                        : null
                };

            return await query.ToListAsync();
        }
    }
}