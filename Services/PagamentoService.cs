using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.DTOs;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;
using EstacionamentoAPI.Services.Interfaces;

namespace EstacionamentoAPI.Services
{
    public class PagamentoService : IPagamentoService
    {
        private readonly IPagamentoRepository _pagamentoRepository;
        private readonly ITarifaService _tarifaService;
        private readonly EstacionamentoContext _context;

        public PagamentoService(
            IPagamentoRepository pagamentoRepository,
            ITarifaService tarifaService,
            EstacionamentoContext context)
        {
            _pagamentoRepository = pagamentoRepository;
            _tarifaService = tarifaService;
            _context = context;
        }

        public async Task<IEnumerable<Pagamento>> GetPagamentos() => await _pagamentoRepository.GetPagamentos();
        public async Task<Pagamento?> GetPagamento(int id) => await _pagamentoRepository.GetById(id);

        // 1. Registra o pagamento aplicando o Lock Pessimista no Banco de Dados
        public async Task<IActionResult> RegistrarPagamento(int idAcesso, DadosSaida dados)
        {
            // Abre transação para segurar o LOCK até o fim do método
            using var transacao = await _context.Database.BeginTransactionAsync();

            try
            {
                // FOR UPDATE impede qualquer outra requisição de ler este Acesso até o Commit
                var acesso = await _context.Acessos
                    .FromSqlRaw("SELECT * FROM \"Acessos\" WHERE \"IdAcesso\" = {0} FOR UPDATE", idAcesso)
                    .Include(a => a.Veiculo)
                    .FirstOrDefaultAsync();

                if (acesso == null)
                {
                    await transacao.RollbackAsync();
                    return new NotFoundObjectResult("Acesso não encontrado.");
                }

                var pagamentoExistente = await _pagamentoRepository.GetByIdAcesso(idAcesso);
                if (pagamentoExistente != null)
                {
                    await transacao.RollbackAsync();
                    return new BadRequestObjectResult("Já existe pagamento registrado para este acesso.");
                }

                if (acesso.HoraSaida != null)
                {
                    await transacao.RollbackAsync();
                    return new BadRequestObjectResult("Este acesso já foi finalizado.");
                }

                if (acesso.Veiculo == null || string.IsNullOrWhiteSpace(acesso.Veiculo.Placa))
                {
                    await transacao.RollbackAsync();
                    return new BadRequestObjectResult("Veículo ou placa não encontrados.");
                }

                acesso.HoraSaida = DateTime.Now;
                acesso.TempoPermanencia = acesso.HoraSaida.Value - acesso.HoraEntrada;

                var tarifaCalculada = await _tarifaService.CalcularTarifaAsync(acesso.Veiculo.Placa, acesso.TempoPermanencia.Value);

                var pagamento = new Pagamento
                {
                    IdAcesso = acesso.IdAcesso,
                    DataHora = DateTime.Now,
                    ValorPago = tarifaCalculada.valorFinal,
                    FormaPagamento = dados.FormaPagamento,
                    StatusPagamento = "Pago"
                };

                await _pagamentoRepository.Add(pagamento);
                await _pagamentoRepository.SaveChanges();

                // Sucesso! Comita e libera o banco
                await transacao.CommitAsync();

                return new OkObjectResult(new { mensagem = "Pagamento registrado com sucesso.", pagamento.IdPagamento, pagamento.ValorPago, acesso.HoraSaida });
            }
            catch (Exception)
            {
                await transacao.RollbackAsync();
                return new StatusCodeResult(500); 
            }
        }

        // 2. Atualizar Pagamento com concorrência otimista (EF Core)
        public async Task<IActionResult> AtualizarPagamento(int id, Pagamento pagamento)
        {
            if (id != pagamento.IdPagamento) return new BadRequestObjectResult("ID do pagamento não corresponde.");

            var pagamentoAtual = await _pagamentoRepository.GetById(id);
            if (pagamentoAtual == null) return new NotFoundObjectResult("Pagamento não encontrado.");

            pagamentoAtual.FormaPagamento = pagamento.FormaPagamento;
            pagamentoAtual.StatusPagamento = pagamento.StatusPagamento;
            pagamentoAtual.ValorPago = pagamento.ValorPago;

            try
            {
                _pagamentoRepository.Update(pagamentoAtual);
                
                // Se o registro mudou no banco desde o GetById, o EF Core joga a exceção aqui
                await _pagamentoRepository.SaveChanges();
                
                return new NoContentResult();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Retorna status 409 (Conflict) indicando que o registro foi modificado por outro processo
                return new ConflictObjectResult("O pagamento foi alterado por outro usuário de forma concorrente.");
            }
        }

        public async Task<IActionResult> ExcluirPagamento(int id)
        {
            var pagamento = await _pagamentoRepository.GetById(id);
            if (pagamento == null) return new NotFoundObjectResult("Pagamento não encontrado.");

            _pagamentoRepository.Remove(pagamento);
            await _pagamentoRepository.SaveChanges();
            return new NoContentResult();
        }
    }
}