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
                            Nombre = fuente.Poblacion?.Provincia ?? "Desconocida"
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
                bool faltabaCP = string.IsNullOrWhiteSpace(nuevoMonumento.CodigoPostal);
                bool faltabaLocalidad = string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Nombre);
                bool faltabaProvincia = string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Provincia.Nombre);

                if (faltabaDireccion) motivos.Add("Faltaba la dirección");
                if (faltabaCP) motivos.Add("Faltaba el código postal");
                if (faltabaLocalidad) motivos.Add("Faltaba la localidad");
                if (faltabaProvincia) motivos.Add("Faltaba la provincia");

                if (faltabaDireccion || faltabaCP || faltabaLocalidad || faltabaProvincia)
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
                    if (faltabaCP && !string.IsNullOrWhiteSpace(postcode))
                    {
                        correcciones.Add($"Se completó el código postal con '{postcode}'");
                        nuevoMonumento.CodigoPostal = postcode;
                    }
                    if (faltabaLocalidad && !string.IsNullOrWhiteSpace(locality))
                    {
                        correcciones.Add($"Se completó la localidad con '{locality}'");
                        nuevoMonumento.Localidad.Nombre = locality;
                    }
                    if (faltabaProvincia && !string.IsNullOrWhiteSpace(province))
                    {
                        correcciones.Add($"Se completó la provincia con '{province}'");
                        nuevoMonumento.Localidad.Provincia.Nombre = province;
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
                    "CLE",
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

        private string ConvertirTipoMonumento(string tipoOriginal)
        {
            if (string.IsNullOrEmpty(tipoOriginal))
                return "Otros";

            return tipoOriginal;
        }
    }
}
