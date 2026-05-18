using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeiculosController : ControllerBase
    {
        private readonly EstacionamentoContext _context;

        public VeiculosController(EstacionamentoContext context)
        {
            _context = context;
        }

        // GET: api/Veiculos (Lista todos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Veiculo>>> GetVeiculos()
        {
            return await _context.Veiculos.Include(v => v.Usuario).ToListAsync();
        }

        // GET: api/Veiculos/usuario/5 (Lista veículos de um dono específico)
        [HttpGet("usuario/{idUsuario}")]
        public async Task<ActionResult<IEnumerable<Veiculo>>> GetVeiculosPorUsuario(int idUsuario)
        {
            return await _context.Veiculos.Where(v => v.IdUsuario == idUsuario).ToListAsync();
        }

        // POST: api/Veiculos
        [HttpPost]
        public async Task<ActionResult<Veiculo>> PostVeiculo(Veiculo veiculo)
        {
            // Verifica duplicidade de placa
            if (await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa))
                return BadRequest("Placa já cadastrada.");

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVeiculos), new { id = veiculo.IdVeiculo }, veiculo);
        }
        
        // DELETE: api/Veiculos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVeiculo(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null) return NotFound();

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}