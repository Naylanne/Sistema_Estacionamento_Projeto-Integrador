using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.Services.Interfaces;

namespace EstacionamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoricoController : ControllerBase
    {
        private readonly IAcessoService _acessoService;

        public HistoricoController(IAcessoService acessoService)
        {
            _acessoService = acessoService;
        }

        // GET: api/Historico
        [HttpGet]
        public async Task<IActionResult> GetHistorico()
        {
            var historico = await _acessoService.GetAcessos();
            return Ok(historico);
        }

        // GET: api/Historico/ativos
        [HttpGet("ativos")]
        public async Task<IActionResult> GetAcessosAtivos()
        {
            var acessosAtivos = await _acessoService.GetAcessosAtivos();
            return Ok(acessosAtivos);
        }

        // GET: api/Historico/finalizados
        [HttpGet("finalizados")]
        public async Task<IActionResult> GetAcessosFinalizados()
        {
            var acessosFinalizados = await _acessoService.GetAcessosFinalizados();
            return Ok(acessosFinalizados);
        }

        // GET: api/Historico/com-pagamento
        [HttpGet("com-pagamento")]
        public async Task<IActionResult> GetHistoricoComPagamento()
        {
            var historico = await _acessoService.GetHistoricoComPagamento();
            return Ok(historico);
        }
    }
}