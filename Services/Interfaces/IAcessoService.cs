using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Services.Interfaces
{
    public interface IAcessoService
    {
        Task<IActionResult> RegistrarEntrada(DadosEntrada dados);
        
        Task<IActionResult> RegistrarSaida(int idAcesso, DadosSaida dados);
    }
}