using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Services.Interfaces;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            var usuarios = await _usuarioService.GetUsuarios();
            return Ok(usuarios);
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _usuarioService.GetUsuario(id);
            if (usuario == null)
            {
                return NotFound("Usuário não encontrado.");
            }
            return Ok(usuario);
        }

        // POST: api/Usuarios
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var resultado = await _usuarioService.Login(login);
            return resultado;
        }

        [HttpPost]
        public async Task<IActionResult> PostUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _usuarioService.RegistrarUsuario(usuario);
            
            if (resultado is OkObjectResult okResult && okResult.Value is Usuario usuarioCriado)
            {
                return CreatedAtAction(nameof(GetUsuario), new { id = usuarioCriado.IdUsuario }, usuarioCriado);
            }

            return resultado;
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuarioAtualizado)
        {
            var resultado = await _usuarioService.AtualizarUsuario(id, usuarioAtualizado);
            return resultado;
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var resultado = await _usuarioService.ExcluirUsuario(id);
            return resultado;
        }
    }
}