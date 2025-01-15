using Iei.Models;

namespace Iei.Services
{
    public interface ICargaService
    {
        Task<Dictionary<string, object>> ImportDataAsync(CargaRequest request);
        Task<object> LlamarMicroservicioValenciaAsync();
        Task<object> LlamarMicroservicioEuskadiAsync();
        Task<object> LlamarMicroservicioCastillaLeonAsync();

    }
}
