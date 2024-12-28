using Iei.Models.Dto;

namespace Iei.Services
{
    public interface ICLEService
    {
        Task<ResultadoExtraccionDto> ImportMonumentosAsync();
    }
}
