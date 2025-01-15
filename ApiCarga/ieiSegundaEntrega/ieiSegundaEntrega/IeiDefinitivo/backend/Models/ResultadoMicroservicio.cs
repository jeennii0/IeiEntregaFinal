namespace Iei.Models
{
    public class ResultadoMicroservicio
    {
            public int NumeroMonumentosInsertados { get; set; }
            public List<MonumentosReparadosDto> MonumentoReparado { get; set; }
            public List<MonumentosRechazadosDto> MonumentoDescartado { get; set; }
        }
    }
