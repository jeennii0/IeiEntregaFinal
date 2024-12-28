
using Iei.Models.Dto;

namespace Iei.Services
{
    public interface ICVService
    {
        Task<ResultadoExtraccionDto> ImportMonumentosAsync();
    }
}