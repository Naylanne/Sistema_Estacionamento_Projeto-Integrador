using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Repositories.Interfaces
{
    public interface ITarifaRepository
    {
        Task<IEnumerable<Tarifa>> GetTarifas();
        Task<Tarifa?> GetById(int id);
        Task SaveChanges();
    }
}