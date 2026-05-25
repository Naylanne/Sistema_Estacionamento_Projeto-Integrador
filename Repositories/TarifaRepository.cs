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

        // Retorna a lista de tarifas para leitura
        public async Task<IEnumerable<Tarifa>> GetTarifas()
        {
            return await _context.Tarifas.ToListAsync();
        }

        // O FindAsync traz o objeto para que o Service possa fazer o cruzamento de concorrência
        public async Task<Tarifa?> GetById(int id)
        {
            return await _context.Tarifas.FindAsync(id);
        }

        // Salva as alterações e o EF injeta o RowVersion no comando SQL
        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}