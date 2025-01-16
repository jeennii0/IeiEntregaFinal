namespace Iei.Models.Dto
{
    public class Response
    { 
          public int MonumentosInsertados { get; set; }

            public List<MonumentosReparadosDto> MonumentosReparados { get; set; }
           = new List<MonumentosReparadosDto>();

            public List<MonumentosRechazadosDto> MonumentosRechazados { get; set; }
                = new List<MonumentosRechazadosDto>();
        }
    }
