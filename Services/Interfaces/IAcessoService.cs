using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Services.Interfaces
{
    public interface IAcessoService
    {
        Task<IEnumerable<Acesso>> GetAcessos();

        Task<IEnumerable<Acesso>> GetAcessosAtivos();

        Task<IEnumerable<Acesso>> GetAcessosFinalizados();

        Task<IActionResult> RegistrarEntrada(DadosEntrada dados);

        Task<IActionResult> RegistrarSaida(int idAcesso, DadosSaida dados);
    
        Task<IEnumerable<HistoricoComPagamentoDto>> GetHistoricoComPagamento();  
    }
}