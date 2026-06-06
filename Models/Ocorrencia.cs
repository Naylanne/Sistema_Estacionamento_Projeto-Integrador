using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EstacionamentoAPI.Models
{
    public class Ocorrencia
    {
        [Key]
        public int IdOcorrencia { get; set; }

        public int IdAcesso { get; set; }

        [Required]
        [Column(TypeName = "timestamp")]
        public DateTime DataHora { get; set; }

        [Required]
        public string Descricao { get; set; } = string.Empty;

        [JsonIgnore]
        public Acesso? Acesso { get; set; }

    }
}