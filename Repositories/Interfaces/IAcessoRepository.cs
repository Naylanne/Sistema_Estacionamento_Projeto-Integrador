using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Repositories.Interfaces
{
    public interface IAcessoRepository
    {
        Task<Vaga?> ObterVagaComLock(
            int idVaga);

        Task<bool> ExisteVeiculoNoPatio(
            string placa);

        Task<bool>
            ExisteOcupacaoAtivaNaVaga(
                int idVaga);

        Task AdicionarAcesso(
            AcessoVeiculo acesso);

        Task<AcessoVeiculo?>
            GetById(
                int idAcesso);

        Task<Vaga?>
            GetVagaById(
                int idVaga);

        Task SaveChanges();
    }
}