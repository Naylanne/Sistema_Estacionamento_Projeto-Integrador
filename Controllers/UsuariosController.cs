using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly EstacionamentoContext _context;

        public UsuariosController(EstacionamentoContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            return usuario;
        }

        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verifica CPF duplicado
            bool cpfExiste = await _context.Usuarios.AnyAsync(u => u.Cpf == usuario.Cpf);
            if (cpfExiste)
            {
                return BadRequest("Já existe um usuário cadastrado com este CPF.");
            }

            // Gera hash da senha
            usuario.SenhaAcesso = BCrypt.Net.BCrypt.HashPassword(usuario.SenhaAcesso);
                        
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, usuario);
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuarioAtualizado)
        {
            if (id != usuarioAtualizado.IdUsuario)
            {
                return BadRequest("ID do usuário não confere.");
            }

            var usuarioBanco = await _context.Usuarios.FindAsync(id);
            if (usuarioBanco == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            // Concorrência Otimista
            _context.Entry(usuarioBanco).Property(u => u.RowVersion).OriginalValue = usuarioAtualizado.RowVersion;

            // Verifica CPF duplicado
            bool cpfExiste = await _context.Usuarios.AnyAsync(u => u.Cpf == usuarioAtualizado.Cpf && u.IdUsuario != id);
            if (cpfExiste)
            {
                return BadRequest("Já existe outro usuário cadastrado com este CPF.");
            }

            // Atualiza campos
            usuarioBanco.TipoUsuario = usuarioAtualizado.TipoUsuario;
            usuarioBanco.Cpf = usuarioAtualizado.Cpf;
            usuarioBanco.Nome = usuarioAtualizado.Nome;
            usuarioBanco.DataNascimento = usuarioAtualizado.DataNascimento;
            usuarioBanco.Cargo = usuarioAtualizado.Cargo;
            usuarioBanco.PlacaCarro = usuarioAtualizado.PlacaCarro;
            usuarioBanco.Telefone = usuarioAtualizado.Telefone;
            usuarioBanco.Endereco = usuarioAtualizado.Endereco;

            // Atualiza senha apenas se enviada
            if (!string.IsNullOrWhiteSpace(usuarioAtualizado.SenhaAcesso))
            {
                usuarioBanco.SenhaAcesso = BCrypt.Net.BCrypt.HashPassword(usuarioAtualizado.SenhaAcesso);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound("Usuário não encontrado.");
                }

                // Retorna HTTP 409 (Conflict) indicando que o registro foi modificado por outro processo
                return Conflict(new
                {
                    mensagem = 
                    "Este cadastro de usuário foi alterado por outro funcionário enquanto você o editava. Por favor, recarregue os dados e tente novamente."
                });
            }

            return NoContent();
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}