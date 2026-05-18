using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.DTOs;
using EstacionamentoAPI.Services.Interfaces;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoricoController
        : ControllerBase
    {
        private readonly
            IAcessoService
            _acessoService;

        public HistoricoController(
            IAcessoService acessoService)
        {
            _acessoService =
                acessoService;
        }

        // POST:
        // api/Historico/entrada
        [HttpPost("entrada")]
        public async Task<IActionResult>
            RegistrarEntrada(
                [FromBody]
                DadosEntrada dados)
        {
            return await
                _acessoService
                    .RegistrarEntrada(
                        dados);
        }

        // POST:
        // api/Historico/saida/1
        [HttpPost("saida/{idAcesso}")]
        public async Task<IActionResult>
            RegistrarSaida(
                int idAcesso,
                [FromBody]
                DadosSaida dados)
        {
            return await
                _acessoService
                    .RegistrarSaida(
                        idAcesso,
                        dados);
        }
    }
}