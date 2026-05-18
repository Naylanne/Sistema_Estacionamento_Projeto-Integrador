using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly EstacionamentoContext _context;

        public FeedbacksController(EstacionamentoContext context)
        {
            _context = context;
        }

        // GET: api/Feedbacks
        // Consultar feedbacks (Para o Admin ver)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Feedback>>> GetFeedbacks()
        {
            // Inclui os dados do usuário que enviou para facilitar a leitura
            return await _context.Feedbacks
                .Include(f => f.Usuario) 
                .ToListAsync();
        }

        // POST: api/Feedbacks
        // Requisito F10: Incluir página de feedback
        [HttpPost]
        public async Task<ActionResult<Feedback>> PostFeedback(Feedback feedback)
        {
            // Verifica se o usuário existe
            var usuario = await _context.Usuarios.FindAsync(feedback.IdUsuario);
            if (usuario == null)
            {
                return BadRequest("Usuário não identificado.");
            }

            feedback.DataEnvio = DateTime.UtcNow; // Força UTC
            
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFeedbacks), new { id = feedback.IdFeedback }, feedback);
        }
    }
}