using Iei.Models;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Iei.Services
{
    public class CargaService : ICargaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MicroservicesOptions _microservicesOptions;

        public CargaService(
            IHttpClientFactory httpClientFactory,
            IOptions<MicroservicesOptions> microservicesOptions)
        {
            _httpClientFactory = httpClientFactory;
            _microservicesOptions = microservicesOptions.Value;
        }
        public async Task<Dictionary<string, object>> ImportDataAsync(CargaRequest cargaRequest)
        {
            var resultadoGlobal = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (cargaRequest.Comunidades.Contains("Valencia", StringComparer.OrdinalIgnoreCase))
            {
                var valenciaResult = await LlamarMicroservicioValenciaAsync();
                resultadoGlobal["Valencia"] = valenciaResult;
            }

            if (cargaRequest.Comunidades.Contains("Euskadi", StringComparer.OrdinalIgnoreCase))
            {
                var euskadiResult = await LlamarMicroservicioEuskadiAsync();
                resultadoGlobal["Euskadi"] = euskadiResult;
            }

            if (cargaRequest.Comunidades.Contains("CastillaLeon", StringComparer.OrdinalIgnoreCase))
            {
                var cylResult = await LlamarMicroservicioCastillaLeonAsync();
                resultadoGlobal["CastillaLeon"] = cylResult;
            }

            return resultadoGlobal;
        }

        public async Task<object> LlamarMicroservicioValenciaAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var urlValencia = _microservicesOptions.Valencia; 

            var response = await client.PostAsync(urlValencia, null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        public async Task<object> LlamarMicroservicioEuskadiAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var urlEuskadi = _microservicesOptions.Euskadi;

            var response = await client.PostAsync(urlEuskadi, null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        public async Task<object> LlamarMicroservicioCastillaLeonAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var urlCastillaLeon = _microservicesOptions.CastillaLeon;

            var response = await client.PostAsync(urlCastillaLeon, null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

    }
}
