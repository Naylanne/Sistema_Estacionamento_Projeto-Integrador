using System.ComponentModel.DataAnnotations;

namespace EstacionamentoAPI.Models
{
    public class Aviso
    {
        [Key]
        public int IdAviso { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descricao { get; set; } = string.Empty;
   
    }
}