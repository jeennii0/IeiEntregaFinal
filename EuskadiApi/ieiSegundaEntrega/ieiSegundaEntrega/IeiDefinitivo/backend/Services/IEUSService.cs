using Iei.Models.Dto;

namespace Iei.Services
{
    public interface IEUSService
    {
        Task<ResultadoExtraccionDto> ImportMonumentosAsync();
    }
}
