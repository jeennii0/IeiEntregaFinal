using System.Text.RegularExpressions;
using System.Web;
using Iei.ModelosFuentesOriginales;
using Iei.Models;
using Iei.Services;
using Iei.Extractors.ValidacionMonumentos;
using Iei.Models.Dto;

namespace Iei.Extractors
{
    public class CLEExtractor
    {
        private readonly GeocodingService geocodingService;

        public CLEExtractor(GeocodingService geocodingService)
        {
            this.geocodingService = geocodingService;
        }

        public async Task<ResultadoExtraccionDto> ExtractData(List<ModeloXMLOriginal> monumentosXml)
        {
            var resultadoExtraccion = new ResultadoExtraccionDto();

            foreach (ModeloXMLOriginal fuente in monumentosXml)
            {

                var motivos = new List<string>();
                var correcciones = new List<string>();
                var errores = new List<string>();
                var provinciaNormalizada = NormalizarProvincia(fuente.Poblacion?.Provincia);

                if (string.IsNullOrWhiteSpace(provinciaNormalizada))
                {
                    errores.Add($"La provincia '{fuente.Poblacion?.Provincia}' no es correcta o no está soportada.");
                    Rechazar(new Monumento
                    {
                        Nombre = fuente.Nombre ?? "Desconocido",
                        Localidad = new Localidad
                        {
                            Nombre = fuente.Poblacion?.Localidad ?? "Desconocida",
                            Provincia = new Provincia { Nombre = fuente.Poblacion?.Provincia ?? "Desconocida" }
                        }
                    }, errores, resultadoExtraccion);
                    continue;
                }

                var nuevoMonumento = new Monumento
                {
                    Nombre = fuente.Nombre ?? "Desconocido",
                    Descripcion = ProcesarDescripcion(fuente.Descripcion),
                    CodigoPostal = fuente.CodigoPostal ?? "",
                    Direccion = fuente.Calle ?? "",
                    Latitud = fuente.Coordenadas?.Latitud ?? 0,
                    Longitud = fuente.Coordenadas?.Longitud ?? 0,
                    Tipo = ConvertirTipoMonumento(fuente.TipoMonumento),
                    Localidad = new Localidad
                    {
                        Nombre = fuente.Poblacion?.Localidad ?? "Desconocida",
                        Provincia = new Provincia
                        {
                            Nombre = NormalizarProvincia(fuente.Poblacion?.Provincia)
                        }
                    }
                };

                if (!ValidacionesMonumentos.EsMonumentoInicialValido(
                    nuevoMonumento.Nombre,
                    nuevoMonumento.Descripcion,
                    errores))
                {
                    Rechazar(nuevoMonumento, errores, resultadoExtraccion);
                    continue;
                }

                if (!ValidacionesMonumentos.ValidarCoordenadas(
                    nuevoMonumento.Latitud,
                    nuevoMonumento.Longitud,
                    errores))
                {
                    Rechazar(nuevoMonumento, errores, resultadoExtraccion);
                    continue;
                }

                bool faltabaDireccion = string.IsNullOrWhiteSpace(nuevoMonumento.Direccion);
                bool faltabaLocalidad = string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Nombre);

                if (faltabaDireccion) motivos.Add("Faltaba la dirección");
                if (faltabaLocalidad) motivos.Add("Faltaba la localidad");

                if (faltabaDireccion || faltabaLocalidad )
                {
                    var (address, postcode, province, locality) =
                        await geocodingService.GetGeocodingDetails(
                            nuevoMonumento.Latitud,
                            nuevoMonumento.Longitud
                        );

                    if (faltabaDireccion && !string.IsNullOrWhiteSpace(address))
                    {
                        correcciones.Add($"Se completó la dirección con '{address}'");
                        nuevoMonumento.Direccion = address;
                    }
                    if (faltabaLocalidad && !string.IsNullOrWhiteSpace(locality))
                    {
                        correcciones.Add($"Se completó la localidad con '{locality}'");
                        nuevoMonumento.Localidad.Nombre = locality;
                    }
                }

                string cpAntes = nuevoMonumento.CodigoPostal;
                if (cpAntes.Length == 4)
                {
                    motivos.Add("El código postal tenía 4 dígitos");
                }

                nuevoMonumento.CodigoPostal = ValidacionesMonumentos.CompletarCodigoPostal(
                    nuevoMonumento.CodigoPostal,
                    correcciones
                );

                if (!ValidacionesMonumentos.SonDatosDireccionValidos(
                    nuevoMonumento.Nombre,
                    nuevoMonumento.CodigoPostal,
                    nuevoMonumento.Direccion,
                    nuevoMonumento.Localidad.Nombre,
                    nuevoMonumento.Localidad.Provincia.Nombre,
                    errores))
                {
                    Rechazar(nuevoMonumento, errores, resultadoExtraccion);
                    continue;
                }

                if (!ValidacionesMonumentos.EsCodigoPostalCorrectoParaRegion(
                    nuevoMonumento.CodigoPostal,
                    errores))
                {
                    Rechazar(nuevoMonumento, errores, resultadoExtraccion);
                    continue;
                }
                bool huboReparacion = (motivos.Count > 0 || correcciones.Count > 0);
                if (huboReparacion)
                {
                    var reparadoDto = new MonumentosReparadosDto
                    {
                        Nombre = nuevoMonumento.Nombre,
                        Localidad = nuevoMonumento.Localidad?.Nombre ?? "",
                        MotivoError = string.Join(" | ", motivos),
                        OperacionRealizada = string.Join(" | ", correcciones)
                    };
                    resultadoExtraccion.MonumentosReparados.Add(reparadoDto);
                }

                resultadoExtraccion.MonumentosInsertados++;
                resultadoExtraccion.MonumentosValidados.Add(nuevoMonumento);
            }


            return resultadoExtraccion;
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

        private string ProcesarDescripcion(string descripcionHtml)
        {
            if (string.IsNullOrWhiteSpace(descripcionHtml))
                return "Desconocida";

            var textoDecodificado = HttpUtility.HtmlDecode(descripcionHtml);
            var textoLimpio = Regex.Replace(textoDecodificado, "<.*?>", string.Empty);
            return textoLimpio.Trim();
        }

        public string ConvertirTipoMonumento(string tipoMonumento)
        {
            var tipoMonumentoMap = new Dictionary<string, string>
            {
                { "Yacimientos arqueológico", "Yacimiento arqueológico" },
                { "Casa", "Edificio singular" },
                { "Casas Nobles", "Edificio singular" },
                { "Ermitas", "Iglesia-Ermita" },
                { "Iglesias", "Iglesia-Ermita" },
                { "Catedral", "Monasterio-Convento" },
                { "Torre", "Castillo-Fortaleza-Torre" },
                { "Muralla", "Castillo-Fortaleza-Torre" },
                { "Castillos", "Castillo-Fortaleza-Torre" },
                { "Puerta", "Castillo-Fortaleza-Torre" },
                { "Palacios", "Edificio singular" },
                { "Puentes", "Puente" },
                { "Santuario", "Monasterio-Convento" },
                { "Monasterios", "Monasterio-Convento" }
            };

            foreach (var key in tipoMonumentoMap.Keys)
            {
                if (!string.IsNullOrEmpty(tipoMonumento) && tipoMonumento.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    return tipoMonumentoMap[key];
                }
            }

            return "Otros";
        }
        private string NormalizarProvincia(string provincia)
        {
            var provinciaMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Burgos", "Burgos" },
                { "Segovia", "Segovia" },
                { "León", "León" },
                { "Leon", "León" },
                { "Ávila", "Ávila" },
                { "Avila", "Ávila" },
                { "Salamanca", "Salamanca" },
                { "Zamora", "Zamora" },
                { "Palencia", "Palencia" },
                { "Valladolid", "Valladolid" },
                { "Soria", "Soria" },

            };

            return provinciaMap.ContainsKey(provincia) ? provinciaMap[provincia] : "";
        }
    }
}
