using Iei.Models;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Iei.Services
{
    public class CargaService : ICargaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MicroservicesOptions _microservicesOptions;
        private readonly IeiContext _ieiContext;

        public CargaService(
            IHttpClientFactory httpClientFactory,
            IOptions<MicroservicesOptions> microservicesOptions,
            IeiContext ieiContext)
        {
            _httpClientFactory = httpClientFactory;
            _microservicesOptions = microservicesOptions.Value;
            _ieiContext = ieiContext;
        }

        public async Task<Dictionary<string, object>> ImportDataAsync(CargaRequest cargaRequest)
        {
            int totalInsertados = 0;
            var registrosReparados = new List<dynamic>();
            var registrosRechazados = new List<dynamic>();

            var resultadoGlobal = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // Importar datos de cada fuente
            if (cargaRequest.Comunidades.Contains("Valencia", StringComparer.OrdinalIgnoreCase))
            {
                var valenciaResult = await LlamarMicroservicioAsync("Valencia", _microservicesOptions.Valencia, registrosReparados, registrosRechazados);
                totalInsertados += valenciaResult.NumeroMonumentosInsertados;
            }

            if (cargaRequest.Comunidades.Contains("Euskadi", StringComparer.OrdinalIgnoreCase))
            {
                var euskadiResult = await LlamarMicroservicioAsync("Euskadi", _microservicesOptions.Euskadi, registrosReparados, registrosRechazados);
                totalInsertados += euskadiResult.NumeroMonumentosInsertados;
            }

            if (cargaRequest.Comunidades.Contains("CastillaLeon", StringComparer.OrdinalIgnoreCase))
            {
                var cylResult = await LlamarMicroservicioAsync("Castilla y León", _microservicesOptions.CastillaLeon, registrosReparados, registrosRechazados);
                totalInsertados += cylResult.NumeroMonumentosInsertados;
            }

            // Crear la respuesta combinada
            var response = new
            {
                NumeroMonumentosInsertados = totalInsertados,
                RegistrosConErroresYReparados = registrosReparados,
                RegistrosConErroresYRechazados = registrosRechazados
            };

            return new Dictionary<string, object> { { "ResultadoCarga", response } };
        }

        public async Task<dynamic> LlamarMicroservicioAsync(string fuente, string url, List<dynamic> registrosReparados, List<dynamic> registrosRechazados)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(url, null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var resultado = Newtonsoft.Json.JsonConvert.DeserializeObject<ResultadoMicroservicio>(content);

            // Agregar registros reparados con la fuente
            foreach (var reparado in resultado.MonumentoReparado)
            {
                registrosReparados.Add(new
                {
                    FuenteDatos = fuente,
                    Nombre = reparado.Nombre,
                    Localidad = reparado.Localidad,
                    MotivoError = reparado.MotivoError,
                    OperacionRealizada = reparado.OperacionRealizada
                });
            }

            // Agregar registros rechazados con la fuente
            foreach (var rechazado in resultado.MonumentoDescartado)
            {
                registrosRechazados.Add(new
                {
                    FuenteDatos = fuente,
                    Nombre = rechazado.Nombre,
                    Localidad = rechazado.Localidad,
                    MotivoError = rechazado.MotivoError
                });
            }

            return new { resultado.NumeroMonumentosInsertados };
        }

        public async Task VaciarBaseDeDatosAsync()
        {
            // Asume que tienes un contexto para interactuar con la base de datos.
            using (var scope = _ieiContext.Database.BeginTransaction())
            {
                try
                {
                    // Eliminar los datos de las tablas principales.
                    _ieiContext.Monumento.RemoveRange(_ieiContext.Monumento);
                    _ieiContext.Provincia.RemoveRange(_ieiContext.Provincia);
                    _ieiContext.Localidad.RemoveRange(_ieiContext.Localidad);

                    // Guardar cambios.
                    await _ieiContext.SaveChangesAsync();

                    // Confirmar la transacción.
                    scope.Commit();
                }
                catch (Exception)
                {
                    // Revertir la transacción en caso de error.
                    scope.Rollback();
                    throw;
                }
            }
        }

    }
}
