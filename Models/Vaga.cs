using System.ComponentModel.DataAnnotations;

namespace EstacionamentoAPI.Models
{
    public class Vaga
    {
        [Key]
        public int IdVaga { get; set; }
        public string TipoVaga { get; set; } = "Carro"; // Carro, Moto
        public string Status { get; set; } = "Disponivel"; // Disponivel, Ocupada

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}