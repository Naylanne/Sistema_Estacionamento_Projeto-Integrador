using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EstacionamentoAPI.Models
{
    public class Feedback
    {
        [Key]
        public int IdFeedback { get; set; }

        public int IdUsuario { get; set; }

        [Required]
        public string Mensagem { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "timestamp")]
        public DateTime DataEnvio { get; set; }

        [JsonIgnore]
        public Usuario? Usuario { get; set; }
    
    }
}