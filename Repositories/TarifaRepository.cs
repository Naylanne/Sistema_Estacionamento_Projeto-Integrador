using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;

namespace EstacionamentoAPI.Repositories
{
    public class TarifaRepository : ITarifaRepository
    {
        private readonly EstacionamentoContext _context;

        public TarifaRepository(EstacionamentoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tarifa>> GetTarifas()
        {
            return await _context.Tarifas.ToListAsync();
        }

        public async Task<Tarifa?> GetById(int id)
        {
            return await _context.Tarifas.FindAsync(id);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}