using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Data;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories.Interfaces;
using EstacionamentoAPI.Services.Interfaces;

namespace EstacionamentoAPI.Services
{
    public class TarifaService : ITarifaService
    {
        private readonly ITarifaRepository _tarifaRepository;
        private readonly EstacionamentoContext _context;

        public TarifaService(
            ITarifaRepository tarifaRepository,
            EstacionamentoContext context)
        {
            _tarifaRepository = tarifaRepository;
            _context = context;
        }

        public async Task<IEnumerable<Tarifa>> GetTarifas()
        {
            return await _tarifaRepository.GetTarifas();
        }

        public async Task<IActionResult> AtualizarTarifa(int id, Tarifa tarifa)
        {
            if (id != tarifa.IdTarifa)
            {
                return new BadRequestObjectResult("ID da tarifa não corresponde.");
            }

            // 1. Busca os dados atuais salvos no banco com o RowVersion para concorrência otimista
            var tarifaAtual = await _tarifaRepository.GetById(id);

            if (tarifaAtual == null)
            {
                return new NotFoundObjectResult("Tarifa não encontrada.");
            }

            // 2. Configura a concorrência otimista informando ao EF o RowVersion original da tela do usuário
            _context.Entry(tarifaAtual).Property(t => t.RowVersion).OriginalValue = tarifa.RowVersion;

           // 3. Atualiza os campos de negócio
           tarifaAtual.ValorPrimeiraHora = tarifa.ValorPrimeiraHora;
           tarifaAtual.ValorHoraAdicional = tarifa.ValorHoraAdicional;
           tarifaAtual.ValorDiaria = tarifa.ValorDiaria;
           tarifaAtual.DescontoFuncionario = tarifa.DescontoFuncionario;
           tarifaAtual.DescontoParceiro = tarifa.DescontoParceiro;

           try
           {
              // 4. Salva as alterações aplicando o WHERE com o RowVersion original
              await _tarifaRepository.SaveChanges();
              return new NoContentResult();
           }
           catch (DbUpdateConcurrencyException)
           {
             // 5. Retorna o HTTP 409 (Conflict) indicando que o registro foi modificado por outro processo
             return new ConflictObjectResult(new
             { mensagem = 
             "Outro usuário modificou a tabela de preços enquanto você editava. Por favor, recarregue a página para obter os valores atuais e tente novamente."
             });
            }
        }

        public async Task<(decimal valorFinal, string tipoAplicado)> CalcularTarifaAsync(string placa, TimeSpan permanencia)
        {
            double totalHoras = Math.Ceiling(permanencia.TotalHours);

            // Mantido AsNoTracking para que o cálculo da saída (pessimista) sempre leia o dado real
            var tarifa = await _context.Tarifas
                .AsNoTracking()
                .FirstOrDefaultAsync() ?? new Tarifa();

            decimal valorFinal = 0;

            if (totalHoras <= 1)
            {
                valorFinal = tarifa.ValorPrimeiraHora;
            }
            else
            {
                valorFinal = tarifa.ValorPrimeiraHora + ((decimal)(totalHoras - 1) * tarifa.ValorHoraAdicional);
            }

            if (valorFinal > tarifa.ValorDiaria)
            {
                valorFinal = tarifa.ValorDiaria;
            }

            var veiculo = await _context.Veiculos
                .Include(v => v.Usuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Placa == placa);

            string tipoAplicado = "Visitante (Sem desconto)";

            if (veiculo?.Usuario != null)
            {
                if (veiculo.Usuario.TipoUsuario == "Funcionario")
                {
                    decimal desconto = valorFinal * (tarifa.DescontoFuncionario / 100);
                    valorFinal -= desconto;
                    tipoAplicado = "Funcionario";
                }
                else if (veiculo.Usuario.TipoUsuario == "Parceiro")
                {
                    decimal desconto = valorFinal * (tarifa.DescontoParceiro / 100);
                    valorFinal -= desconto;
                    tipoAplicado = "Parceiro";
                }
            }

            return (valorFinal, tipoAplicado);
        }
    }
}