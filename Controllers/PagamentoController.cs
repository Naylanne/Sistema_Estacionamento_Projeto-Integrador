using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.DTOs;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Services.Interfaces;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagamentoController : ControllerBase
    {
        private readonly IPagamentoService _pagamentoService;

        public PagamentoController(IPagamentoService pagamentoService)
        {
            _pagamentoService = pagamentoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pagamento>>> GetPagamentos()
        {
            var pagamentos = await _pagamentoService.GetPagamentos();
            return Ok(pagamentos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pagamento>> GetPagamento(int id)
        {
            var pagamento = await _pagamentoService.GetPagamento(id);

            if (pagamento == null)
            {
                return NotFound("Pagamento não encontrado.");
            }

            return Ok(pagamento);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPagamento(
            int id,
            Pagamento pagamento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return await _pagamentoService
                .AtualizarPagamento(id, pagamento);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePagamento(int id)
        {
            return await _pagamentoService.ExcluirPagamento(id);
        }
    }
}