namespace Iei.Models.Dto
{
    public class ResultadoExtraccionDto
    {
        public int MonumentosInsertados;
        public List<Monumento> MonumentosValidados { get; set; }
           = new List<Monumento>();
        public List<MonumentosReparadosDto> MonumentosReparados { get; set; }
       = new List<MonumentosReparadosDto>();

        public List<MonumentosRechazadosDto> MonumentosRechazados { get; set; }
            = new List<MonumentosRechazadosDto>();
    }
}
