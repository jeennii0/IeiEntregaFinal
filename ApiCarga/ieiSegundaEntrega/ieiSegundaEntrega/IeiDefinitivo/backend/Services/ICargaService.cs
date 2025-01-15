using Iei.Models;

namespace Iei.Services
{
    public interface ICargaService
    {
        Task<Dictionary<string, object>> ImportDataAsync(CargaRequest request);
        Task<dynamic> LlamarMicroservicioAsync(string fuente, string url, List<dynamic> registrosReparados, List<dynamic> registrosRechazados);

    }
}
