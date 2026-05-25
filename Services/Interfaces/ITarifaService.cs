using Microsoft.AspNetCore.Mvc;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Services.Interfaces
{
    public interface ITarifaService
    {
        Task<IEnumerable<Tarifa>> GetTarifas();

        Task<IActionResult> AtualizarTarifa(
            int id, Tarifa tarifa);

        Task<(decimal valorFinal, string tipoAplicado)>
            CalcularTarifaAsync(string placa, TimeSpan permanencia);
    }
}