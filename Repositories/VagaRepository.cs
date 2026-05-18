using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;

namespace EstacionamentoAPI.Repositories
{
    public class VagaRepository : IVagaRepository
    {
        private readonly EstacionamentoContext _context;

    public VagaRepository(EstacionamentoContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Vaga>> GetAll()
    {
        return await _context.Vagas.ToListAsync();
    }

    public async Task<Vaga?> GetById(int id)
    {
        return await _context.Vagas.FindAsync(id);
    }

    public async Task<Vaga?> BuscarComLock(int id)
    {
        return await _context.Vagas
            .FromSqlRaw(@"
                SELECT * FROM ""Vagas""
                WHERE ""IdVaga"" = {0}
                FOR UPDATE", id)
            .FirstOrDefaultAsync();
    }

    public async Task Add(Vaga vaga)
    {
        _context.Vagas.Add(vaga);
        await _context.SaveChangesAsync();
    }

    public void Remove(Vaga vaga)
    {
        _context.Vagas.Remove(vaga);
    }
    }
}