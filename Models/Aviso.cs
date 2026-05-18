using System.ComponentModel.DataAnnotations;

namespace EstacionamentoAPI.Models
{
    public class Aviso
    {
        [Key]
        public int IdAviso { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
    }
}