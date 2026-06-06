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

        [HttpGet]
        public async Task<IActionResult> GetAcessos()
        {
            var acessos = await _acessoService.GetAcessos();
            return Ok(acessos);
        }

        [HttpPost("entrada")]
        public async Task<IActionResult> RegistrarEntrada(DadosEntrada dados)
        {
            return await _acessoService.RegistrarEntrada(dados);
        }
    }
}