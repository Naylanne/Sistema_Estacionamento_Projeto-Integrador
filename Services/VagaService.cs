using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;
using EstacionamentoAPI.Services.Interfaces;

namespace EstacionamentoAPI.Services
{
    public class VagaService : IVagaService
    {
        private readonly IVagaRepository _vagaRepository;
        private readonly EstacionamentoContext _context;

        public VagaService(
            IVagaRepository vagaRepository,
            EstacionamentoContext context)
        {
            _vagaRepository = vagaRepository;
            _context = context;
        }

        public async Task<IEnumerable<Vaga>> GetVagas()
        {
            return await _vagaRepository.GetAll();
        }

        public async Task<Vaga?> GetVaga(int id)
        {
            return await _vagaRepository.GetById(id);
        }

        public async Task<Vaga> CriarVaga(Vaga vaga)
        {
            if (string.IsNullOrEmpty(vaga.TipoVaga))
                vaga.TipoVaga = "Carro";

            if (string.IsNullOrEmpty(vaga.Status))
                vaga.Status = "Disponivel";

            await _vagaRepository.Add(vaga);
            return vaga;
        }

        public async Task<IActionResult> AtualizarVaga(int id, Vaga vaga)
        {
            if (id != vaga.IdVaga)
                return new BadRequestObjectResult("ID inconsistente");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // Lock pessimista
                var vagaAtual =
                    await _vagaRepository.BuscarComLock(id);

                if (vagaAtual == null)
                    return new NotFoundObjectResult("Vaga não encontrada");

                // Regra de negócio
                if (vagaAtual.Status == "Ocupada" &&
                    vaga.Status == "Ocupada")
                {
                    return new ConflictObjectResult("Vaga já ocupada");
                }

                vagaAtual.TipoVaga = vaga.TipoVaga;
                vagaAtual.Status = vaga.Status;

                // Concorrência otimista
                _context.Entry(vagaAtual)
                    .Property(v => v.RowVersion)
                    .OriginalValue = vaga.RowVersion;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();

                    return new ConflictObjectResult(
                        "Conflito de concorrência");
                }

                await transaction.CommitAsync();
                return new NoContentResult();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IActionResult> DeletarVaga(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // Lock pessimista
                var vaga =
                    await _vagaRepository.BuscarComLock(id);

                if (vaga == null)
                    return new NotFoundObjectResult(
                        "Vaga não encontrada");

                if (vaga.Status == "Ocupada")
                    return new BadRequestObjectResult(
                        "Vaga ocupada");

                _vagaRepository.Remove(vaga);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new NoContentResult();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                return new ConflictObjectResult(
                    "Conflito de concorrência ao excluir a vaga");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Registrar erro (temporário)
                Console.WriteLine(ex.Message);

                return new ObjectResult(
                    "Erro interno no servidor")
                {
                    StatusCode = 500
                };
            }
        }
    }
}