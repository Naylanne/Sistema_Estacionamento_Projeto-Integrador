using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvisosController : ControllerBase
    {
        private readonly EstacionamentoContext _context;

        public AvisosController(EstacionamentoContext context)
        {
            _context = context;
        }

        // GET: api/Avisos (Requisito F9)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aviso>>> GetAvisos()
        {
            return await _context.Avisos.ToListAsync();
        }

        // POST: api/Avisos (Requisito F17)
        [HttpPost]
        public async Task<ActionResult<Aviso>> PostAviso(Aviso aviso)
        {
            _context.Avisos.Add(aviso);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAvisos), new { id = aviso.IdAviso }, aviso);
        }

        // DELETE: api/Avisos/5 (Requisito F17)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAviso(int id)
        {
            var aviso = await _context.Avisos.FindAsync(id);
            if (aviso == null) return NotFound();

            _context.Avisos.Remove(aviso);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}