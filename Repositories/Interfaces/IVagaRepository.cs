using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Repositories.Interfaces
{
    public interface IVagaRepository
    {
    Task<IEnumerable<Vaga>> GetAll();
    Task<Vaga?> GetById(int id);
    Task<Vaga?> BuscarComLock(int id);
    Task Add(Vaga vaga);
    void Remove(Vaga vaga);
    }
}