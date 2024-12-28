using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Iei.ModelosFuentesOriginales;
using Iei.Models;
using Iei.Services;
using Iei.Extractors.ValidacionMonumentos;

namespace Iei.Extractors
{
    public class CLEExtractor
    {
        private GeocodingService geocodingService = new GeocodingService();

        public async Task<List<Monumento>> ExtractData(List<ModeloXMLOriginal> monumentosXml)
        {
            var monumentos = new List<Monumento>();

            foreach (ModeloXMLOriginal monumento in monumentosXml)
            {
                var errores = new List<string>();
                var correcciones = new List<string>();

                // Crear el nuevo objeto Monumento
                var nuevoMonumento = new Monumento
                {
                    Nombre = monumento.Nombre?.ToString() ?? "Desconocido",
                    Direccion = monumento.Calle?.ToString() ?? "",
                    CodigoPostal = monumento.CodigoPostal?.ToString() ?? "",
                    Descripcion = ProcesarDescripcion(monumento.Descripcion?.ToString() ?? ""),
                    Latitud = monumento.Coordenadas?.Latitud ?? 0,
                    Longitud = monumento.Coordenadas?.Longitud ?? 0,
                    Tipo = ConvertirTipoMonumento(monumento.TipoMonumento),
                    Localidad = new Localidad
                    {
                        Nombre = monumento.Poblacion?.Localidad?.ToString() ?? "Desconocida",
                        Provincia = new Provincia
                        {
                            Nombre = monumento.Poblacion?.Provincia?.ToString() ?? "Desconocida"
                        }
                    }
                };

                // Validar nombre y descripción
                if (!ValidacionesMonumentos.EsMonumentoInicialValido(nuevoMonumento.Nombre, nuevoMonumento.Descripcion, errores))
                {
                    Console.WriteLine($"Rechazado: {string.Join("; ", errores)}");
                    continue;
                }

                // Validar coordenadas geográficas
                if (!ValidacionesMonumentos.ValidarCoordenadas(nuevoMonumento.Latitud, nuevoMonumento.Longitud, errores))
                {
                    Console.WriteLine($"Rechazado: {string.Join("; ", errores)}");
                    continue;
                }

                // Completar datos faltantes con geocodificación
                if (string.IsNullOrWhiteSpace(nuevoMonumento.Direccion) ||
                    string.IsNullOrWhiteSpace(nuevoMonumento.CodigoPostal) ||
                    string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Nombre) ||
                    string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Provincia.Nombre))
                {
                    var (address, postcode, province, locality) = await geocodingService.GetGeocodingDetails(nuevoMonumento.Latitud, nuevoMonumento.Longitud);

                    if (string.IsNullOrEmpty(nuevoMonumento.Direccion)) nuevoMonumento.Direccion = address;
                    if (string.IsNullOrEmpty(nuevoMonumento.CodigoPostal)) nuevoMonumento.CodigoPostal = postcode;
                    if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Nombre)) nuevoMonumento.Localidad.Nombre = locality;
                    if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Provincia.Nombre)) nuevoMonumento.Localidad.Provincia.Nombre = province;
                }

                // Corregir el código postal si es de 4 dígitos
                nuevoMonumento.CodigoPostal = ValidacionesMonumentos.CompletarCodigoPostal(nuevoMonumento.CodigoPostal, correcciones);

                // Validar dirección y código postal
                string localidad = nuevoMonumento.Localidad.Nombre;
                string provincia = nuevoMonumento.Localidad.Provincia.Nombre;
                if (!ValidacionesMonumentos.SonDatosDireccionValidos(nuevoMonumento.Nombre, nuevoMonumento.CodigoPostal, nuevoMonumento.Direccion, localidad, provincia, errores))
                {
                    Console.WriteLine($"Rechazado: {string.Join("; ", errores)}");
                    continue;
                }

                // Validar códigos postales específicos de Castilla y León
                if (!ValidacionesMonumentos.EsCodigoPostalCorrectoParaRegion(nuevoMonumento.CodigoPostal, "CLE", errores))
                {
                    Console.WriteLine($"Rechazado: {string.Join("; ", errores)}");
                    continue;
                }

                // Si todas las validaciones pasan, registrar correcciones y añadir a la lista final
                Console.WriteLine($"Corregido: {string.Join("; ", correcciones)}");
                monumentos.Add(nuevoMonumento);
            }

            return monumentos;
        }

        public string ConvertirTipoMonumento(string tipoMonumento)
        {
            var tipoMonumentoMap = new Dictionary<string, string>
            {
                { "Yacimientos arqueológico", "Yacimientos arqueológicos" },
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

        private string ProcesarDescripcion(string descripcionHtml)
        {
            if (string.IsNullOrWhiteSpace(descripcionHtml))
                return "Desconocida";

            var textoDecodificado = HttpUtility.HtmlDecode(descripcionHtml);
            var textoLimpio = Regex.Replace(textoDecodificado, "<.*?>", string.Empty);

            return textoLimpio.Trim();
        }
    }
}
