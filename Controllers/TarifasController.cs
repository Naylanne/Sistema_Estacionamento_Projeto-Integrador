using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Services.Interfaces;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarifasController : ControllerBase
    {
        private readonly ITarifaService _tarifaService;

        public TarifasController(ITarifaService tarifaService)
        {
            _tarifaService = tarifaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tarifa>>> GetTarifas()
        {
            return Ok(await _tarifaService.GetTarifas());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tarifa>> GetTarifa(int id)
        {
            var tarifa = await _tarifaService.GetTarifa(id);

            if (tarifa == null)
                return NotFound(new { mensagem = "Tarifa não encontrada." });

            return Ok(tarifa);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTarifa(int id, Tarifa tarifa)
        {
            return await _tarifaService.AtualizarTarifa(id, tarifa);
        }
    }
}