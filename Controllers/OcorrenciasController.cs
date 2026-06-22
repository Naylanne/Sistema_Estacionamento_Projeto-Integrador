using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OcorrenciasController : ControllerBase
    {
        private readonly EstacionamentoContext _context;

        public OcorrenciasController(EstacionamentoContext context)
        {
            _context = context;
        }

        // GET: api/Ocorrencias
        // Requisito F8: Listar ocorrências
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ocorrencia>>> GetOcorrencias()
        {
            return await _context.Ocorrencias.ToListAsync();
        }

        // POST: api/Ocorrencias
        // Requisito F8: Incluir registro de ocorrências
        [HttpPost]
        public async Task<ActionResult<Ocorrencia>> PostOcorrencia(Ocorrencia ocorrencia)
        {
            // Valida se o Acesso (Ticket) existe
            var acesso = await _context.Acessos.FindAsync(ocorrencia.IdAcesso);
            if (acesso == null)
            {
                return BadRequest("Ticket de acesso não encontrado. A ocorrência deve estar vinculada a um veículo.");
            }

            ocorrencia.DataHora = DateTime.Now; // Força UTC para PostgreSQL
            
            _context.Ocorrencias.Add(ocorrencia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOcorrencias), new { id = ocorrencia.IdOcorrencia }, ocorrencia);
        }

        // PUT: api/Ocorrencias/5
        // Requisito F15: Alterar registros de intercorrência
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOcorrencia(int id, Ocorrencia ocorrencia)
        {
            if (id != ocorrencia.IdOcorrencia) return BadRequest();

            _context.Entry(ocorrencia).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Ocorrencias/5
        // Requisito F16: Excluir registro de intercorrência
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOcorrencia(int id)
        {
            var ocorrencia = await _context.Ocorrencias.FindAsync(id);
            if (ocorrencia == null) return NotFound();

            _context.Ocorrencias.Remove(ocorrencia);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}