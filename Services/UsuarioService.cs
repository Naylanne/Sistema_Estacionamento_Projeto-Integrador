using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;
using EstacionamentoAPI.Services.Interfaces;
using EstacionamentoAPI.DTOs;
using static BCrypt.Net.BCrypt;

namespace EstacionamentoAPI.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IEnumerable<Usuario>> GetUsuarios() => await _usuarioRepository.GetAll();

        public async Task<Usuario?> GetUsuario(int id) => await _usuarioRepository.GetById(id);

        public async Task<IActionResult> Login(LoginDTO login)
        {
            var usuario = await _usuarioRepository.BuscarPorCpf(login.Cpf);

            if (usuario == null)
            return new UnauthorizedObjectResult("CPF ou senha incorretos.");

            var senhaValida = BCrypt.Net.BCrypt.Verify(
            login.SenhaAcesso,
            usuario.SenhaAcesso
            );

            if (!senhaValida)
            return new UnauthorizedObjectResult("CPF ou senha incorretos.");
 
            return new OkObjectResult(usuario);
        }

        public async Task<IActionResult> RegistrarUsuario(Usuario usuario)
        {
            var cpfExiste = await _usuarioRepository.GetByCpf(usuario.Cpf);
            if (cpfExiste != null)
            {
                return new BadRequestObjectResult("Já existe um usuário cadastrado com este CPF.");
            }

            // Gera hash da senha usando o BCrypt
            usuario.SenhaAcesso = HashPassword(usuario.SenhaAcesso);

            await _usuarioRepository.Add(usuario);
            await _usuarioRepository.SaveChanges();

            return new OkObjectResult(usuario);
        }

        public async Task<IActionResult> AtualizarUsuario(int id, Usuario usuarioAtualizado)
        {
            if (id != usuarioAtualizado.IdUsuario)
            {
                return new BadRequestObjectResult("ID do usuário não confere.");
            }

            var usuarioBanco = await _usuarioRepository.GetById(id);
            if (usuarioBanco == null)
            {
                return new NotFoundObjectResult("Usuário não encontrado.");
            }

            // Aplica a Concorrência Otimista passando o valor original que veio da requisição
            _usuarioRepository.SetOriginalRowVersion(usuarioBanco, usuarioAtualizado.RowVersion);

            // Valida CPF duplicado descartando o próprio usuário que está sendo editado
            var usuarioComMesmoCpf = await _usuarioRepository.GetByCpf(usuarioAtualizado.Cpf);
            if (usuarioComMesmoCpf != null && usuarioComMesmoCpf.IdUsuario != id)
            {
                return new BadRequestObjectResult("Já existe outro usuário cadastrado com este CPF.");
            }

            // Atualiza os campos da entidade rastreada
            usuarioBanco.TipoUsuario = usuarioAtualizado.TipoUsuario;
            usuarioBanco.Cpf = usuarioAtualizado.Cpf;
            usuarioBanco.Nome = usuarioAtualizado.Nome;
            usuarioBanco.DataNascimento = usuarioAtualizado.DataNascimento;
            usuarioBanco.Cargo = usuarioAtualizado.Cargo;
            usuarioBanco.Telefone = usuarioAtualizado.Telefone;
            usuarioBanco.Endereco = usuarioAtualizado.Endereco;

            if (!string.IsNullOrWhiteSpace(usuarioAtualizado.SenhaAcesso))
            {
                usuarioBanco.SenhaAcesso = HashPassword(usuarioAtualizado.SenhaAcesso);
            }

            try
            {
                await _usuarioRepository.SaveChanges();
                return new NoContentResult();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_usuarioRepository.Exists(id))
                {
                    return new NotFoundObjectResult("Usuário não encontrado.");
                }

                // Captura e retorna o Conflito 409 
                return new ConflictObjectResult(new
                {
                    mensagem = "Este cadastro de usuário foi alterado por outro funcionário enquanto você o editava. Por favor, recarregue os dados e tente novamente."
                });
            }
        }

        public async Task<IActionResult> ExcluirUsuario(int id)
        {
            var usuario = await _usuarioRepository.GetById(id);
            if (usuario == null)
            {
                return new NotFoundObjectResult("Usuário não encontrado.");
            }

            _usuarioRepository.Remove(usuario);
            await _usuarioRepository.SaveChanges();

            return new NoContentResult();
        }
    }
}