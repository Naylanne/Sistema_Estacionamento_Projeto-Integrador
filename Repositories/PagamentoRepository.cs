using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;

namespace EstacionamentoAPI.Repositories
{
    public class PagamentoRepository : IPagamentoRepository
    {
        private readonly EstacionamentoContext _context;

        public PagamentoRepository(EstacionamentoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pagamento>> GetPagamentos()
        {
            return await _context.Pagamentos
                .Include(p => p.Acesso)
                .ToListAsync();
        }

        public async Task<Pagamento?> GetById(int id)
        {
            return await _context.Pagamentos
                .Include(p => p.Acesso)
                .FirstOrDefaultAsync(p => p.IdPagamento == id);
        }

        public async Task<Pagamento?> GetByIdAcesso(int idAcesso)
        {
            return await _context.Pagamentos
                .Include(p => p.Acesso)
                .FirstOrDefaultAsync(p => p.IdAcesso == idAcesso);
        }

        public async Task Add(Pagamento pagamento)
        {
            await _context.Pagamentos.AddAsync(pagamento);
        }

        public void Update(Pagamento pagamento)
        {
            _context.Pagamentos.Update(pagamento);
        }

        public void Remove(Pagamento pagamento)
        {
            _context.Pagamentos.Remove(pagamento);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}