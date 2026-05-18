using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EstacionamentoAPI.Models
{
    public class Veiculo
    {
        [Key]
        public int IdVeiculo { get; set; }

        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string TipoVeiculo { get; set; } = "Carro"; // Carro, Moto

        // Chave Estrangeira para o Dono
        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        [JsonIgnore] // Evita ciclo infinito no JSON
        public Usuario? Usuario { get; set; }
    }
}