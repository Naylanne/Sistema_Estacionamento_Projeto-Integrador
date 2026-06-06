using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Repositories.Interfaces
{
    public interface IPagamentoRepository
    {
        Task<IEnumerable<Pagamento>> GetPagamentos();

        Task<Pagamento?> GetById(int id);

        Task<Pagamento?> GetByIdAcesso(int idAcesso);

        Task Add(Pagamento pagamento);

        void Update(Pagamento pagamento);

        void Remove(Pagamento pagamento);

        Task SaveChanges();
    }
}