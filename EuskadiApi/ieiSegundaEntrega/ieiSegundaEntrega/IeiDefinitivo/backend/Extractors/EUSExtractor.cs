using System.Web;
using Iei.Modelos_Fuentes;
using Iei.Models;
using Iei.Models.Dto;
using Iei.Extractors.ValidacionMonumentos;

public class EUSExtractor
{
    public async Task<ResultadoExtraccionDto> ExtractData(List<ModeloJSONOriginal> monumentosJson)
    {
        var resultadoExtraccion = new ResultadoExtraccionDto();
        var loteDuplicados = new HashSet<(string, double, double)>(); // Detección de duplicados dentro del lote

        foreach (var fuente in monumentosJson)
        {
            var motivos = new List<string>();
            var correcciones = new List<string>();
            var errores = new List<string>();

            // Crear el objeto Monumento
            var nuevoMonumento = new Monumento
            {
                Nombre = fuente.DocumentName ?? "Desconocido",
                Descripcion = ProcesarDescripcion(fuente.DocumentDescription),
                CodigoPostal = fuente.PostalCode ?? "",
                Direccion = fuente.Address ?? "",
                Latitud = fuente.Latwgs84,
                Longitud = fuente.Lonwgs84,
                Tipo =  ConvertirTipoMonumento(fuente.DocumentName)
                Localidad = new Localidad
                {
                    Nombre = fuente.Municipality ?? "Desconocida",
                    Provincia = new Provincia
                    {
                        Nombre = fuente.Territory ?? "Desconocida"
                    }
                }
            };

            // Validar datos iniciales (nombre, descripción)
            if (!ValidacionesMonumentos.EsMonumentoInicialValido(nuevoMonumento.Nombre, nuevoMonumento.Descripcion, errores))
            {
                Rechazar(nuevoMonumento, errores, resultadoExtraccion);
                continue;
            }

            // Validar coordenadas
            if (!ValidacionesMonumentos.ValidarCoordenadas(nuevoMonumento.Latitud, nuevoMonumento.Longitud, errores))
            {
                Rechazar(nuevoMonumento, errores, resultadoExtraccion);
                continue;
            }

            // Duplicados dentro del lote
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

            // Completar y validar código postal
            if (nuevoMonumento.CodigoPostal.Length == 4)
            {
                motivos.Add("El código postal tenía 4 dígitos");
            }
            nuevoMonumento.CodigoPostal = ValidacionesMonumentos.CompletarCodigoPostal(nuevoMonumento.CodigoPostal, correcciones);

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

            // Añadir reparaciones si las hubo
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

            // Añadir monumento a los validados
            resultadoExtraccion.MonumentosInsertados++;
            resultadoExtraccion.MonumentosValidados.Add(nuevoMonumento);
        }

        return resultadoExtraccion;
    }

    private void Rechazar(Monumento mon, List<string> errores, ResultadoExtraccionDto resultadoExtraccionDto)
    {
        var motivo = string.Join("; ", errores);
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
                { "Castillo", "Castillo-Fortaleza-Torre" },
                { "Ermita", "Iglesia-Ermita" },
                { "Monasterio", "Monasterio-Convento" },
                { "Torre", "Castillo-Fortaleza-Torre" },
                { "Palacio", "Edificio singular" },
                { "Catedral", "Iglesia-Ermita" },
                { "Puente", "Puente" },
                { "Iglesia", "Iglesia-Ermita" },
                { "Basílica", "Iglesia-Ermita" },
                { "Ayuntamiento", "Edificio singular" },
                { "Casa-Torre", "Castillo-Fortaleza-Torre" },
                { "Convento", "Monasterio-Convento" },
                { "Muralla", "Castillo-Fortaleza-Torre" },
                { "Parroquia", "Iglesia-Ermita" },
                { "Santuario", "Iglesia-Ermita" },
                { "Teatro", "Edificio singular" },
                { "Torre-Palacio", "Castillo-Fortaleza-Torre" },
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

    private string ProcesarDescripcion(string descripcionHtml)
    {
        if (string.IsNullOrWhiteSpace(descripcionHtml))
            return "Desconocida";

        var textoDecodificado = HttpUtility.HtmlDecode(descripcionHtml);
        var textoLimpio = System.Text.RegularExpressions.Regex.Replace(textoDecodificado, "<.*?>", string.Empty);
        return textoLimpio.Trim();
    }
}
