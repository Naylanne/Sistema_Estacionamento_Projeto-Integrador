using System.ComponentModel.DataAnnotations;

namespace EstacionamentoAPI.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        public string TipoUsuario { get; set; } = string.Empty; // Cliente, Funcionario, Parceiro
        
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter exatamente 11 números.")]
        public string Cpf { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;

        public DateTime DataNascimento { get; set; }

        public string Cargo { get; set; } = string.Empty; // Atendente, Gerente

        public string PlacaCarro { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public string Endereco { get; set; } = string.Empty;

        public string SenhaAcesso { get; set; } = string.Empty;

        public ICollection<Veiculo>? Veiculos { get; set; }

        public uint RowVersion { get; set; }
    }
}