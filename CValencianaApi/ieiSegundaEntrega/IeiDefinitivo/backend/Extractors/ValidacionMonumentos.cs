namespace Iei.Extractors.ValidacionMonumentos
{
    public static class ValidacionesMonumentos
    {
        // Valida los datos iniciales del monumento
        public static bool EsMonumentoInicialValido(string nombre, string descripcion, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                errores.Add("Se descarta el monumento: no tiene nombre.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                errores.Add($"Se descarta el monumento '{nombre}': la descripción no es válida.");
                return false;
            }

            return true;
        }

        // Valida los datos relacionados con la dirección y el código postal del monumento
        public static bool SonDatosDireccionValidos(string nombre, string codigoPostal, string direccion, string localidad, string provincia, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                errores.Add($"Se descarta el monumento '{nombre}': el código postal está vacío.");
                return false;
            }

            if (!EsCodigoPostalValido(codigoPostal, errores))
            {
                errores.Add($"Se descarta el monumento '{nombre}': el código postal '{codigoPostal}' no es válido.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(provincia))
            {
                errores.Add($"Se descarta el monumento '{nombre}': la provincia está vacía.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(direccion))
            {
                errores.Add($"Se descarta el monumento '{nombre}': la dirección está vacía.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(localidad))
            {
                errores.Add($"Se descarta el monumento '{nombre}': la localidad está vacía.");
                return false;
            }

            return true;
        }

        // Completa un código postal de 4 dígitos con un 0 al inicio
        public static string CompletarCodigoPostal(string codigoPostal, List<string> correcciones)
        {
            codigoPostal = codigoPostal.Trim();
            if (codigoPostal.Length == 4)
            {
                correcciones.Add($"El código postal '{codigoPostal}' fue completado con un '0' al inicio.");
                return "0" + codigoPostal;
            }
            return codigoPostal;
        }

        // Valida si un código postal tiene el formato correcto
        public static bool EsCodigoPostalValido(string codigoPostal, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                errores.Add("La API de Geocodificación no ha podido obtener el código postal.");
                return false;
            }

            codigoPostal = codigoPostal.Trim();

            if (!System.Text.RegularExpressions.Regex.IsMatch(codigoPostal, @"^\d{5}$"))
            {
                errores.Add($"El código postal '{codigoPostal}' no tiene un formato válido.");
                return false;
            }
            if(!(codigoPostal.StartsWith("03") || codigoPostal.StartsWith("12") || codigoPostal.StartsWith("46"))){
                errores.Add($"El código postal '{codigoPostal}' no es válido para la región.");
                return false;
            }

            return true;
        }

        // Valida las coordenadas UTM del monumento
        public static bool EsCoordenadaUtmValida(double? utmEste, double? utmNorte, List<string> errores)
        {
            if (!utmEste.HasValue || !utmNorte.HasValue)
            {
                errores.Add("Las coordenadas UTM están incompletas: faltan valores de Este o Norte.");
                return false;
            }

            if (double.IsNaN(utmEste.Value) || double.IsNaN(utmNorte.Value))
            {
                errores.Add("Las coordenadas UTM contienen valores no numéricos.");
                return false;
            }

            if (utmEste.Value == 0 || utmNorte.Value == 0)
            {
                errores.Add("Las coordenadas UTM no pueden ser cero.");
                return false;
            }

            return true;
        }

        // Valida si las coordenadas geográficas están en un rango aceptable
        public static bool ValidarCoordenadas(double latitud, double longitud, List<string> errores)
        {
            if (latitud < -90 || latitud > 90)
            {
                errores.Add($"La latitud '{latitud}' está fuera del rango permitido (-90 a 90).");
                return false;
            }

            if (longitud < -180 || longitud > 180)
            {
                errores.Add($"La longitud '{longitud}' está fuera del rango permitido (-180 a 180).");
                return false;
            }

            return true;
        }
    }
}
