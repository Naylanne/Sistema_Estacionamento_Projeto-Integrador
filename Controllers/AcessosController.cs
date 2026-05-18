using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.DTOs;
using EstacionamentoAPI.Services.Interfaces;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcessosController : ControllerBase
    {
        private readonly
            IAcessoService _acessoService;

        public AcessosController(
            IAcessoService acessoService)
        {
            _acessoService =
                acessoService;
        }

        // POST: api/Acessos/entrada
        [HttpPost("entrada")]
        public async Task<IActionResult>
            RegistrarEntrada(
                DadosEntrada dados)
        {
            return await
                _acessoService
                    .RegistrarEntrada(
                        dados);
        }
    }
}