using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Services.Interfaces;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VagasController : ControllerBase
    {
        private readonly IVagaService _vagaService;

        public VagasController(IVagaService vagaService)
        {
            _vagaService = vagaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vaga>>> GetVagas()
        {
            return Ok(await _vagaService.GetVagas());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Vaga>> GetVaga(int id)
        {
            var vaga = await _vagaService.GetVaga(id);

            if (vaga == null)
                return NotFound(new { mensagem = "Vaga não encontrada." });

            return Ok(vaga);
        }

        [HttpPost]
        public async Task<ActionResult> PostVaga(Vaga vaga)
        {
            var novaVaga = await _vagaService.CriarVaga(vaga);

            return CreatedAtAction(nameof(GetVaga),
                new { id = novaVaga.IdVaga }, novaVaga);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVaga(int id, Vaga vaga)
        {
            return await _vagaService.AtualizarVaga(id, vaga);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVaga(int id)
        {
            return await _vagaService.DeletarVaga(id);
        }
    }
}