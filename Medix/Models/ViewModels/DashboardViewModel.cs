using Medix.Models;

namespace Medix.ViewModels
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public int TotalUnidades { get; set; }
        public int UnidadesAtivas { get; set; }
        public int UnidadesSuspensas { get; set; }
        public int UnidadesInativas { get; set; }
        public List<UnidadeMedica> AtividadeRecente { get; set; } = new List<UnidadeMedica>();
        public Dictionary<string, int> StatusDistribution { get; set; } = new Dictionary<string, int>();
    }
}