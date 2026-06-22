using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.DTOs;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;
using EstacionamentoAPI.Services.Interfaces;
using EstacionamentoAPI.Enums;

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
                    return new NotFoundObjectResult("Acesso não encontrado."); // Retorna 404 (o registro de acesso não existe)
                }

                var pagamentoExistente = await _pagamentoRepository.GetByIdAcesso(idAcesso);
                if (pagamentoExistente != null)
                {
                    await transacao.RollbackAsync();
                    return new BadRequestObjectResult("Já existe pagamento registrado para este acesso."); // Retorna 400 (ex.: duplo clique, para evitar que o cliente tente pagar duas vezes o mesmo acesso)
                }

                if (acesso.HoraSaida != null)
                {
                    await transacao.RollbackAsync();
                    return new BadRequestObjectResult("Este acesso já foi finalizado."); // Retorna 400 (acesso já finalizado)
                }

                if (acesso.Veiculo == null || string.IsNullOrWhiteSpace(acesso.Veiculo.Placa))
                {
                    await transacao.RollbackAsync();
                    return new BadRequestObjectResult("Veículo ou placa não encontrados."); // Retorna 400 (veículo ou placa inválidos)
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
                    StatusPagamento = StatusPagamentoEnum.Concluido,
                    RowVersion = 0 // O valor inicial da RowVersion é 0, o banco irá atualizar para o valor correto no INSERT
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
                return new StatusCodeResult(500); // erro inesperado do servidor
            }
        }

        // 2. Atualizar Pagamento com concorrência otimista (EF Core)
        public async Task<IActionResult> AtualizarPagamento(int id, Pagamento pagamento)
        {
            if (id != pagamento.IdPagamento) return new BadRequestObjectResult("ID do pagamento não corresponde.");

            var pagamentoAtual = await _pagamentoRepository.GetById(id);
            if (pagamentoAtual == null) return new NotFoundObjectResult("Pagamento não encontrado.");

             // O EF Core compara com a RowVersion atual no banco para detectar concorrência
            _context.Entry(pagamentoAtual)
                .Property(p => p.RowVersion)
                .OriginalValue = pagamento.RowVersion;

            pagamentoAtual.FormaPagamento = pagamento.FormaPagamento;
            pagamentoAtual.StatusPagamento = pagamento.StatusPagamento;
            pagamentoAtual.ValorPago = pagamento.ValorPago;

            try
            {
                // Não é necessário chamar Update em uma entidade já rastreada.
                // O EF Core já conhece o estado e usa o OriginalValue do RowVersion.
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
            if (pagamento == null) return new NotFoundObjectResult("Pagamento não encontrado."); // Retorna 404 (o registro de pagamento não existe)
            _pagamentoRepository.Remove(pagamento);
            await _pagamentoRepository.SaveChanges();
            return new NoContentResult();
        }
    }
}