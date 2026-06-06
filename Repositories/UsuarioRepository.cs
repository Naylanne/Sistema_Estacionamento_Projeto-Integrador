using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;

namespace EstacionamentoAPI.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly EstacionamentoContext _context;

        public UsuarioRepository(EstacionamentoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usuario>> GetAll() => await _context.Usuarios.ToListAsync();

        public async Task<Usuario?> GetById(int id) => await _context.Usuarios.FindAsync(id);

        public async Task<Usuario?> GetByCpf(string cpf) => await _context.Usuarios.FirstOrDefaultAsync(u => u.Cpf == cpf);

        public async Task<Usuario?> BuscarPorCpf(string cpf) => await _context.Usuarios.FirstOrDefaultAsync(u => u.Cpf == cpf);

        public async Task Add(Usuario usuario) => await _context.Usuarios.AddAsync(usuario);

        public void Remove(Usuario usuario) => _context.Usuarios.Remove(usuario);

        public async Task SaveChanges() => await _context.SaveChangesAsync();

        public bool Exists(int id) => _context.Usuarios.Any(e => e.IdUsuario == id);

        // Define a versão original vinda do cliente para o EF Core fazer a checagem de concorrência
        public void SetOriginalRowVersion(Usuario usuario, uint originalVersion)
        {
            _context.Entry(usuario).Property(u => u.RowVersion).OriginalValue = originalVersion;
        }
    }
}