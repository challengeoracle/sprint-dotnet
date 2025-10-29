namespace Medix.Models
{
    public enum StatusUnidade
    {
        Ativa,
        Inativa,
        Suspensa,
        EmTeste
    }

    public class UnidadeMedica
    {
        public int Id { get; set; } // Chave primária gerada automaticamente

        public string Nome { get; set; }

        public string CNPJ { get; set; }

        public string? Endereco { get; set; }

        public string? Telefone { get; set; }

        public string EmailAdmin { get; set; }

        public StatusUnidade Status { get; set; }

        public DateTime DataCadastro { get; set; }
    }
}