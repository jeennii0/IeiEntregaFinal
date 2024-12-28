namespace Iei.Models.Dto
{
    public class ResultadoCargaDto
    {
        public int NumeroRegistrosCorrectos { get; set; }
        public List<RegistroCorregidoDto> RegistrosCorregidos { get; set; }
        public List<RegistroRechazadoDto> RegistrosRechazados { get; set; }
    }
}
