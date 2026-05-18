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
            return await _tarifaRepository
                .GetTarifas();
        }

        public async Task<IActionResult> AtualizarTarifa(
            int id,
            Tarifa tarifa)
        {
            if (id != tarifa.IdTarifa)
            {
                return new BadRequestObjectResult(
                    "ID da tarifa não corresponde.");
            }

            var tarifaAtual =
                await _tarifaRepository.GetById(id);

            if (tarifaAtual == null)
            {
                return new NotFoundObjectResult(
                    "Tarifa não encontrada.");
            }

            tarifaAtual.ValorPrimeiraHora =
                tarifa.ValorPrimeiraHora;

            tarifaAtual.ValorHoraAdicional =
                tarifa.ValorHoraAdicional;

            tarifaAtual.ValorDiaria =
                tarifa.ValorDiaria;

            tarifaAtual.DescontoFuncionario =
                tarifa.DescontoFuncionario;

            tarifaAtual.DescontoParceiro =
                tarifa.DescontoParceiro;

            try
            {
                await _tarifaRepository
                    .SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ConflictObjectResult(
                    new
                    {
                        mensagem =
                        "Conflito de concorrência ao atualizar tarifa."
                    });
            }

            return new NoContentResult();
        }

        public async Task<(decimal valorFinal,
            string tipoAplicado)>
            CalcularTarifaAsync(
                string placa,
                TimeSpan permanencia)
       {
            double totalHoras =
                Math.Ceiling(permanencia.TotalHours);
  
            var tarifa =
                await _context.Tarifas
                    .FirstOrDefaultAsync()
                ?? new Tarifa();

            decimal valorFinal = 0;

           // cálculo base
           if (totalHoras <= 1)
           {
               valorFinal =
                   tarifa.ValorPrimeiraHora;
           }
           else
           {
               valorFinal =
                   tarifa.ValorPrimeiraHora +
                   ((decimal)(totalHoras - 1)
                   * tarifa.ValorHoraAdicional);
           }

           // aplica diária
           if (valorFinal >
               tarifa.ValorDiaria)
           {
               valorFinal =
                   tarifa.ValorDiaria;
           }

          // busca veículo + usuário
          var veiculo =
              await _context.Veiculos
                  .Include(v => v.Usuario)
                  .FirstOrDefaultAsync(
                      v => v.Placa == placa);

          string tipoAplicado =
              "Visitante (Sem desconto)";

          if (veiculo?.Usuario != null)
          {
              if (veiculo.Usuario.TipoUsuario
                  == "Funcionario")
              {
                  decimal desconto =
                      valorFinal *
                     (tarifa
                     .DescontoFuncionario
                     / 100);

                  valorFinal -= desconto;

                  tipoAplicado =
                      "Funcionario";
              }
              else if (
                  veiculo.Usuario.TipoUsuario
                  == "Parceiro")
             {
                  decimal desconto =
                      valorFinal *
                     (tarifa
                     .DescontoParceiro
                     / 100);

                  valorFinal -= desconto;

                  tipoAplicado =
                      "Parceiro";
             }
          }

          return (
              valorFinal,
              tipoAplicado
          );
        }
    }
}