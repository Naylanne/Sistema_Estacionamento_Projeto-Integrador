using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAll();
        Task<Usuario?> BuscarPorCpf(string cpf);
        Task<Usuario?> GetById(int id);
        Task<Usuario?> GetByCpf(string cpf);
        Task Add(Usuario usuario);
        void Remove(Usuario usuario);
        Task SaveChanges();
        bool Exists(int id);
        void SetOriginalRowVersion(Usuario usuario, uint originalVersion);
    }
}