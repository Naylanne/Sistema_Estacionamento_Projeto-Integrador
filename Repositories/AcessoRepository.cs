using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;

namespace EstacionamentoAPI.Repositories
{
    public class AcessoRepository : IAcessoRepository
    {
        private readonly EstacionamentoContext _context;

        public AcessoRepository(EstacionamentoContext context)
        {
            _context = context;
        }

        // Busca a primeira vaga disponível para o tipo de veículo e já aplica o Lock
        // Impede que duas requisições peguem a mesma vaga simultaneamente.
        public async Task<Vaga?> ObterPrimeiraVagaDisponivelComLock(string tipoVeiculo)
        {
            return await _context.Vagas
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM vaga
                    WHERE status = 'Disponivel'
                      AND LOWER(tipo_vaga) = LOWER({tipoVeiculo})
                    ORDER BY id_vaga
                    LIMIT 1
                    FOR UPDATE
                ")
                .AsTracking()
                .FirstOrDefaultAsync();
        }

        // Busca o ticket de acesso aplicando Lock para evitar cliques duplos / concorrência na saída.
        public async Task<Acesso?> GetByIdComLock(int idAcesso)
        {
            return await _context.Acessos
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM acesso_veiculo
                    WHERE ""id_acesso"" = {idAcesso}
                    FOR UPDATE")
                .AsTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<Vaga?> ObterVagaComLock(int idVaga)
        {
            return await _context.Vagas
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM ""vaga""
                    WHERE ""id_vaga"" = {idVaga}
                    FOR UPDATE")
                .AsTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExisteVeiculoNoPatio(string placa)
        {
            return await _context.Acessos
                .Include(a => a.Veiculo)
                .AnyAsync(a =>  
                     a.Veiculo != null &&
                     a.Veiculo.Placa == placa &&
                     a.HoraSaida == null);
        }

        public async Task<bool> ExisteOcupacaoAtivaNaVaga(int idVaga)
        {
            return await _context.Acessos
                .AnyAsync(a => a.IdVaga == idVaga && a.HoraSaida == null);
        }

        public async Task AdicionarAcesso(Acesso acesso)
        {
            await _context.Acessos.AddAsync(acesso);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}