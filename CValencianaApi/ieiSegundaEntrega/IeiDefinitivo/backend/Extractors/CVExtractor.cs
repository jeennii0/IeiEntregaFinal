using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iei.Models;
using Iei.Extractors.ValidacionMonumentos;
using Convertidor;
using Iei.Services;
using Iei.Models.Dto;

namespace Iei.Extractors
{
    public class CVExtractor
    {
        private readonly GeocodingService geocodingService;

        public CVExtractor(GeocodingService geocodingService)
        {
            this.geocodingService = geocodingService;
        }

        public async Task<ResultadoExtraccionDto> ExtractData(List<ModeloCSVOriginal> monumentosCsv)
        {
            var resultadoExtraccion = new ResultadoExtraccionDto();
            var convertidor = new Convertidor.Convertidor();
            var loteDuplicados = new HashSet<(string, double, double)>(); // Duplicados en el mismo lote

            foreach (var fuente in monumentosCsv)
            {
                var motivos = new List<string>();
                var correcciones = new List<string>();
                var errores = new List<string>();
                var provinciaNormalizada = NormalizarProvincia(fuente.Provincia);
                if (string.IsNullOrWhiteSpace(provinciaNormalizada))
                {
                    errores.Add($"La provincia '{fuente.Provincia}' no es correcta o no está soportada.");
                    Rechazar(new Monumento
                    {
                        Nombre = fuente.Denominacion ?? "Desconocido",
                        Localidad = new Localidad
                        {
                            Nombre = fuente.Municipio,
                            Provincia = new Provincia { Nombre = fuente.Provincia }
                        }
                    }, errores, resultadoExtraccion);
                    continue;
                }


                // Crear el objeto Monumento
                var nuevoMonumento = new Monumento
                {
                    Nombre = fuente.Denominacion ?? "Desconocido",
                    Descripcion = fuente.Clasificacion ?? "Desconocida",
                    Tipo = ConvertirTipoMonumento(fuente.Categoria),
                    Localidad = new Localidad
                    {
                        Nombre = fuente.Municipio ?? "Desconocida",
                        Provincia = new Provincia
                        {
                            Nombre = NormalizarProvincia(fuente.Provincia)
                        }
                    }
                };

                // Validar nombre y descripción
                if (!ValidacionesMonumentos.EsMonumentoInicialValido(nuevoMonumento.Nombre, nuevoMonumento.Descripcion, errores))
                {
                    Rechazar(nuevoMonumento, errores, resultadoExtraccion);
                    continue;
                }
                // Validar coordenadas UTM
                if (!await AsignarCoordenadasAsync(nuevoMonumento, fuente, convertidor, motivos, errores))
                {
                    Rechazar(nuevoMonumento, errores, resultadoExtraccion);
                    continue;
                }

                // Detectar duplicados en el lote
                var clave = (nuevoMonumento.Nombre, nuevoMonumento.Latitud, nuevoMonumento.Longitud);
                if (loteDuplicados.Contains(clave))
                {
                    var motivo = "Monumento duplicado en el mismo lote (mismo nombre y coordenadas)";
                    resultadoExtraccion.MonumentosRechazados.Add(new MonumentosRechazadosDto
                    {
                        Nombre = nuevoMonumento.Nombre,
                        Localidad = nuevoMonumento.Localidad?.Nombre ?? "",
                        MotivoError = motivo
                    });
                    continue;
                }
                loteDuplicados.Add(clave);

                // Validar dirección y código postal
                if (!await AsignarGeocodificacionAsync(nuevoMonumento, motivos, errores))
                {
                    Rechazar(nuevoMonumento, errores, resultadoExtraccion);
                    continue;
                }
                if (!ValidacionesMonumentos.EsCodigoPostalValido(nuevoMonumento.CodigoPostal, errores))
                {
                    Rechazar(nuevoMonumento, errores, resultadoExtraccion);
                    continue;
                }


                // Si hubo reparaciones, añadir a la lista de reparados
                if (motivos.Count > 0 || correcciones.Count > 0)
                {
                    resultadoExtraccion.MonumentosReparados.Add(new MonumentosReparadosDto
                    {
                        Nombre = nuevoMonumento.Nombre,
                        Localidad = nuevoMonumento.Localidad?.Nombre ?? "",
                        MotivoError = string.Join(" | ", motivos),
                        OperacionRealizada = string.Join(" | ", correcciones)
                    });
                }

                // Agregar monumento a los validados
                resultadoExtraccion.MonumentosInsertados++;
                resultadoExtraccion.MonumentosValidados.Add(nuevoMonumento);
            }

            return resultadoExtraccion;
        }

        private async Task<bool> AsignarCoordenadasAsync(
            Monumento nuevoMonumento,
            ModeloCSVOriginal fuente,
            Convertidor.Convertidor convertidor,
            List<string> motivos,
            List<string> errores)
        {
            try
            {
                if (!ValidacionesMonumentos.EsCoordenadaUtmValida(fuente.UtmEste, fuente.UtmNorte, errores))
                {
                    return false;
                }

                var coordenadas = await convertidor.ConvertUTMToLatLong(fuente.UtmEste.ToString(), fuente.UtmNorte.ToString());
                nuevoMonumento.Latitud = coordenadas.latitud;
                nuevoMonumento.Longitud = coordenadas.longitud;
                return true;
            }
            catch (Exception ex)
            {
                errores.Add($"Error al convertir coordenadas UTM: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> AsignarGeocodificacionAsync(
            Monumento nuevoMonumento,
            List<string> motivos,
            List<string> errores)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nuevoMonumento.Direccion) ||
                    string.IsNullOrWhiteSpace(nuevoMonumento.CodigoPostal) ||
                    string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Nombre) ||
                    string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Provincia.Nombre))
                {
                    var (address, postcode, province, locality) = await geocodingService.GetGeocodingDetails(nuevoMonumento.Latitud, nuevoMonumento.Longitud);

                    if (string.IsNullOrWhiteSpace(nuevoMonumento.Direccion) && !string.IsNullOrWhiteSpace(address))
                    {
                        motivos.Add("Se completó la dirección mediante geocodificación");
                        nuevoMonumento.Direccion = address;
                    }
                    if (string.IsNullOrWhiteSpace(nuevoMonumento.CodigoPostal) && !string.IsNullOrWhiteSpace(postcode))
                    {
                        motivos.Add("Se completó el código postal mediante geocodificación");
                        nuevoMonumento.CodigoPostal = postcode;
                    }
                    if (string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Nombre) && !string.IsNullOrWhiteSpace(locality))
                    {
                        motivos.Add("Se completó la localidad mediante geocodificación");
                        nuevoMonumento.Localidad.Nombre = locality;
                    }
                    if (string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Provincia.Nombre) && !string.IsNullOrWhiteSpace(province))
                    {
                        motivos.Add("Se completó la provincia mediante geocodificación");
                        nuevoMonumento.Localidad.Provincia.Nombre = province;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errores.Add($"Error en la geocodificación: {ex.Message}");
                return false;
            }
        }

        private void Rechazar(Monumento mon, List<string> errores, ResultadoExtraccionDto resultadoExtraccionDto)
        {
            string motivo = string.Join("; ", errores);
            resultadoExtraccionDto.MonumentosRechazados.Add(new MonumentosRechazadosDto
            {
                Nombre = mon.Nombre,
                Localidad = mon.Localidad?.Nombre ?? "",
                MotivoError = motivo
            });
        }

        public string ConvertirTipoMonumento(string tipoMonumento)
        {
            var tipoMonumentoMap = new Dictionary<string, string>
            {
                { "Zona arqueológica", "Yacimiento arqueológico" },
                { "Monumento", "Edificio singular" },
                { "Individual (mueble)", "Edificio singular" },
                { "Conjunto histórico", "Edificio singular" },
                { "Fondo de museo (primera)", "Otros" },
                { "Zona paleontológica", "Yacimiento arqueológico" },
                { "Archivo", "Otros" },
                { "Espacio etnológico", "Otros" },
                { "Sitio histórico", "Edificio singular" },
                { "Jardín histórico", "Edificio singular" },
                { "Parque cultural", "Otros" },
                { "Monumento de interés local", "Edificio singular" }
            };

            return tipoMonumentoMap.ContainsKey(tipoMonumento)
                ? tipoMonumentoMap[tipoMonumento]
                : "Otros";
        }

        private string NormalizarProvincia(string provincia)
        {
            var provinciaMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Alicante", "Alicante" },
                { "Castellon", "Castellón" },
                { "Castellón", "Castellón" },
                { "València", "Valencia" },
                { "Valencia", "Valencia" }
            };
            return provinciaMap.ContainsKey(provincia) ? provinciaMap[provincia] : "";
        }
    }
}
