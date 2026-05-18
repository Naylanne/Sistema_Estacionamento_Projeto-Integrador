using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstacionamentoAPI.Models
{
    public class Feedback
    {
        [Key]
        public int IdFeedback { get; set; }
        public int IdUsuario { get; set; } // FK para Usuario
        
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; } // Propriedade de navegação
        
        public string Mensagem { get; set; } = string.Empty;
        public DateTime DataEnvio { get; set; }
    }
}