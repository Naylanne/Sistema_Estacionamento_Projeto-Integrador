using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.DTOs;
using EstacionamentoAPI.Services.Interfaces;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcessosController : ControllerBase
    {
        private readonly IAcessoService _acessoService;

        public AcessosController(IAcessoService acessoService)
        {
            _acessoService = acessoService;
        }

        // GET: api/Acessos
        [HttpGet]
        public async Task<IActionResult> GetAcessos()
        {
            var acessos = await _acessoService.GetAcessos();
            return Ok(acessos);
        }

        // POST: api/Acessos/entrada
        [HttpPost("entrada")]
        public async Task<IActionResult> RegistrarEntrada([FromBody] DadosEntrada dados)
        {
            return await _acessoService.RegistrarEntrada(dados);
        }

        // POST: api/Acessos/saida/1
        [HttpPost("saida/{idAcesso}")]
        public async Task<IActionResult> RegistrarSaida(
            int idAcesso,
            [FromBody] DadosSaida dados)
        {
            return await _acessoService.RegistrarSaida(idAcesso, dados);
        }
    }
}