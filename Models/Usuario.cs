using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EstacionamentoAPI.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        
        // --- CAMPOS ADICIONADOS CONFORME PDF PÁG. 4 e 19 ---
        public string Cnh { get; set; } = string.Empty; 
        public string Cargo { get; set; } = string.Empty; // Ex: Atendente, Gerente
        // ---------------------------------------------------

        public string TipoUsuario { get; set; } = string.Empty; // Cliente, Funcionario, Parceiro
        public string Endereco { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string SenhaAcesso { get; set; } = string.Empty;

        public ICollection<Veiculo>? Veiculos { get; set; }
    }
}