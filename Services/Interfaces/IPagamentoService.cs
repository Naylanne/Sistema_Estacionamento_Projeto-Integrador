using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Services.Interfaces
{
    public interface IPagamentoService
    {
        Task<IEnumerable<Pagamento>> GetPagamentos();

        Task<Pagamento?> GetPagamento(int id);

        Task<IActionResult> RegistrarPagamento(int idAcesso, DadosSaida dadosSaida);

        Task<IActionResult> AtualizarPagamento(int id, Pagamento pagamento);

        Task<IActionResult> ExcluirPagamento(int id);
        
    }
}