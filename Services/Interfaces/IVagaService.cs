using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Services.Interfaces
{
    public interface IVagaService
   {
    Task<IEnumerable<Vaga>> GetVagas();
    Task<Vaga?> GetVaga(int id);
    Task<Vaga> CriarVaga(Vaga vaga);
    Task<IActionResult> AtualizarVaga(int id, Vaga vaga);
    Task<IActionResult> DeletarVaga(int id);
    }
}