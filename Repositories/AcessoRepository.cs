using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;

namespace EstacionamentoAPI.Repositories
{
    public class AcessoRepository
        : IAcessoRepository
    {
        private readonly
            EstacionamentoContext _context;

        public AcessoRepository(
            EstacionamentoContext context)
        {
            _context = context;
        }

        public async Task<Vaga?>
            ObterVagaComLock(int idVaga)
        {
            return await _context.Vagas
                .FromSqlRaw(@"
                    SELECT *
                    FROM ""Vagas""
                    WHERE ""IdVaga"" = {0}
                    FOR UPDATE",
                    idVaga)
                .FirstOrDefaultAsync();
        }

        public async Task<bool>
            ExisteVeiculoNoPatio(
                string placa)
        {
            return await _context.Acessos
                .AnyAsync(a =>
                    a.Placa == placa
                    && a.HoraSaida == null);
        }

        public async Task<bool>
            ExisteOcupacaoAtivaNaVaga(
                int idVaga)
        {
            return await _context.Acessos
                .AnyAsync(a =>
                    a.IdVaga == idVaga
                    && a.HoraSaida == null);
        }

        public async Task
            AdicionarAcesso(
                AcessoVeiculo acesso)
        {
            await _context.Acessos
                .AddAsync(acesso);
        }
       
        public async Task<AcessoVeiculo?>
            GetById(
                int idAcesso)
        {
            return await _context.Acessos
                .FirstOrDefaultAsync(
                    a => a.IdAcesso
                    == idAcesso);
        }

        public async Task<Vaga?>
            GetVagaById(
                int idVaga)
        {
            return await _context.Vagas
                .FirstOrDefaultAsync(
                    v => v.IdVaga
                    == idVaga);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}