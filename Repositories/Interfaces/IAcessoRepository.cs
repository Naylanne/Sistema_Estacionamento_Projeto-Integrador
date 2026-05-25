using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Repositories.Interfaces
{
    public interface IAcessoRepository
{
    Task<Vaga?> ObterPrimeiraVagaDisponivelComLock(string tipoVeiculo);
    Task<AcessoVeiculo?> GetByIdComLock(int idAcesso);
    Task<Vaga?> ObterVagaComLock(int idVaga);
    Task<bool> ExisteVeiculoNoPatio(string placa);
    Task AdicionarAcesso(AcessoVeiculo acesso);
    Task SaveChanges();
}
}