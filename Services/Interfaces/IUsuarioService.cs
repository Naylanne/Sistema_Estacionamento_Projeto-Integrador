using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.DTOs;

namespace EstacionamentoAPI.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<Usuario>> GetUsuarios();
        Task<Usuario?> GetUsuario(int id);
        Task<IActionResult> Login(LoginDTO login);
        Task<IActionResult> RegistrarUsuario(Usuario usuario);
        Task<IActionResult> AtualizarUsuario(int id, Usuario usuarioAtualizado);
        Task<IActionResult> ExcluirUsuario(int id);
    }
}