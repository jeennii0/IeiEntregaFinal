using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Iei.Modelos_Fuentes;
using Iei.Models;
using Iei.Services;
using Iei.Wrappers;
using Iei.Extractors.ValidacionMonumentos;

namespace Iei.Extractors
{
    public class EUSExtractor
    {
        public EUSWrapper JsonWrapper = new EUSWrapper();
        private GeocodingService GeocodingService = new GeocodingService();

        public async Task<List<Monumento>> ExtractDataAsync(List<ModeloJSONOriginal> monumentosJson)
        {
            try
            {
                var monumentos = new List<Monumento>();

                foreach (ModeloJSONOriginal monumentoJson in monumentosJson)
                {
                    var errores = new List<string>();
                    var correcciones = new List<string>();

                    // Validar datos iniciales
                    if (!ValidacionesMonumentos.EsMonumentoInicialValido(monumentoJson.DocumentName, monumentoJson.DocumentDescription, errores))
                    {
                        Console.WriteLine($"Rechazado: {string.Join("; ", errores)}");
                        continue;
                    }

                    // Validar coordenadas geográficas
                    if (!ValidacionesMonumentos.ValidarCoordenadas(monumentoJson.Latwgs84, monumentoJson.Lonwgs84, errores))
                    {
                        Console.WriteLine($"Rechazado: {string.Join("; ", errores)}");
                        continue;
                    }

                    // Crear el nuevo objeto Monumento
                    var nuevoMonumento = new Monumento
                    {
                        Nombre = monumentoJson.DocumentName?.ToString() ?? "Desconocido",
                        Direccion = monumentoJson.Address?.ToString() ?? "",
                        CodigoPostal = monumentoJson.PostalCode?.ToString() ?? "",
                        Descripcion = monumentoJson.DocumentDescription?.ToString() ?? "Sin descripción",
                        Latitud = monumentoJson.Latwgs84,
                        Longitud = monumentoJson.Lonwgs84,
                        Tipo = ConvertirTipoMonumento(monumentoJson.DocumentName),
                        Localidad = new Localidad
                        {
                            Nombre = monumentoJson.Municipality?.ToString() ?? "Desconocida",
                            Provincia = new Provincia
                            {
                                Nombre = monumentoJson.Territory?.ToString() ?? "Desconocida"
                            }
                        }
                    };

                    // Geocodificación si faltan datos
                    if (string.IsNullOrWhiteSpace(nuevoMonumento.Direccion) ||
                        string.IsNullOrWhiteSpace(nuevoMonumento.CodigoPostal) ||
                        string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Nombre) ||
                        string.IsNullOrWhiteSpace(nuevoMonumento.Localidad.Provincia.Nombre))
                    {
                        var (address, postcode, province, locality) = await GeocodingService.GetGeocodingDetails(nuevoMonumento.Latitud, nuevoMonumento.Longitud);

                        if (string.IsNullOrEmpty(nuevoMonumento.Direccion)) nuevoMonumento.Direccion = address;
                        if (string.IsNullOrEmpty(nuevoMonumento.CodigoPostal)) nuevoMonumento.CodigoPostal = postcode;
                        if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Nombre)) nuevoMonumento.Localidad.Nombre = locality;
                        if (string.IsNullOrEmpty(nuevoMonumento.Localidad.Provincia.Nombre)) nuevoMonumento.Localidad.Provincia.Nombre = province;
                    }

                    // Completar código postal si es necesario
                    nuevoMonumento.CodigoPostal = ValidacionesMonumentos.CompletarCodigoPostal(nuevoMonumento.CodigoPostal, correcciones);

                    // Validar dirección y código postal
                    string localidad = nuevoMonumento.Localidad.Nombre;
                    string provincia = nuevoMonumento.Localidad.Provincia.Nombre;
                    if (!ValidacionesMonumentos.SonDatosDireccionValidos(nuevoMonumento.Nombre, nuevoMonumento.CodigoPostal, nuevoMonumento.Direccion, localidad, provincia, errores))
                    {
                        Console.WriteLine($"Rechazado: {string.Join("; ", errores)}");
                        continue;
                    }

                    // Validar códigos postales específicos para Euskadi
                    if (!ValidacionesMonumentos.EsCodigoPostalCorrectoParaRegion(nuevoMonumento.CodigoPostal, "EUS", errores))
                    {
                        Console.WriteLine($"Rechazado: {string.Join("; ", errores)}");
                        continue;
                    }

                    // Si pasa todas las validaciones, agregar a la lista
                    Console.WriteLine($"Corregido: {string.Join("; ", correcciones)}");
                    monumentos.Add(nuevoMonumento);
                }

                return monumentos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al extraer datos: {ex.Message}");
                return null;
            }
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
                { "Torre-Palacio", "Castillo-Fortaleza-Torre" }
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
    }
}
