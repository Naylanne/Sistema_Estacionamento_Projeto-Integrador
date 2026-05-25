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
                    FROM ""Vagas""
                    WHERE ""Status"" = 'Disponivel'
                      AND LOWER(""TipoVaga"") = LOWER({tipoVeiculo})
                    LIMIT 1
                    FOR UPDATE")
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        // Busca o ticket de acesso aplicando Lock para evitar cliques duplos / concorrência na saída.
        public async Task<AcessoVeiculo?> GetByIdComLock(int idAcesso)
        {
            return await _context.Acessos
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM ""Acessos""
                    WHERE ""IdAcesso"" = {idAcesso}
                    FOR UPDATE")
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<Vaga?> ObterVagaComLock(int idVaga)
        {
            return await _context.Vagas
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM ""Vagas""
                    WHERE ""IdVaga"" = {idVaga}
                    FOR UPDATE")
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExisteVeiculoNoPatio(string placa)
        {
            return await _context.Acessos
                .AnyAsync(a => a.Placa == placa && a.HoraSaida == null);
        }

        public async Task<bool> ExisteOcupacaoAtivaNaVaga(int idVaga)
        {
            return await _context.Acessos
                .AnyAsync(a => a.IdVaga == idVaga && a.HoraSaida == null);
        }

        public async Task AdicionarAcesso(AcessoVeiculo acesso)
        {
            await _context.Acessos.AddAsync(acesso);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}