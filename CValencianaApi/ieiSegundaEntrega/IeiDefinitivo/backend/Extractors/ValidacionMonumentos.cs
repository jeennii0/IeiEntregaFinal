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
                errores.Add($"Se descarta el monumento '{nombre}': la descripci�n no es v�lida.");
                return false;
            }

            return true;
        }

        // Valida los datos relacionados con la direcci�n y el c�digo postal del monumento
        public static bool SonDatosDireccionValidos(string nombre, string codigoPostal, string direccion, string localidad, string provincia, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                errores.Add($"Se descarta el monumento '{nombre}': el c�digo postal est� vac�o.");
                return false;
            }

            if (!EsCodigoPostalValido(codigoPostal, errores))
            {
                errores.Add($"Se descarta el monumento '{nombre}': el c�digo postal '{codigoPostal}' no es v�lido.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(provincia))
            {
                errores.Add($"Se descarta el monumento '{nombre}': la provincia est� vac�a.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(direccion))
            {
                errores.Add($"Se descarta el monumento '{nombre}': la direcci�n est� vac�a.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(localidad))
            {
                errores.Add($"Se descarta el monumento '{nombre}': la localidad est� vac�a.");
                return false;
            }

            return true;
        }

        // Completa un c�digo postal de 4 d�gitos con un 0 al inicio
        public static string CompletarCodigoPostal(string codigoPostal, List<string> correcciones)
        {
            codigoPostal = codigoPostal.Trim();
            if (codigoPostal.Length == 4)
            {
                correcciones.Add($"El c�digo postal '{codigoPostal}' fue completado con un '0' al inicio.");
                return "0" + codigoPostal;
            }
            return codigoPostal;
        }

        // Valida si un c�digo postal tiene el formato correcto
        public static bool EsCodigoPostalValido(string codigoPostal, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                errores.Add("La API de Geocodificaci�n no ha podido obtener el c�digo postal.");
                return false;
            }

            codigoPostal = codigoPostal.Trim();

            if (!System.Text.RegularExpressions.Regex.IsMatch(codigoPostal, @"^\d{5}$"))
            {
                errores.Add($"El c�digo postal '{codigoPostal}' no tiene un formato v�lido.");
                return false;
            }
            if(!(codigoPostal.StartsWith("03") || codigoPostal.StartsWith("12") || codigoPostal.StartsWith("46"))){
                errores.Add($"El c�digo postal '{codigoPostal}' no es v�lido para la regi�n.");
                return false;
            }

            return true;
        }

        // Valida las coordenadas UTM del monumento
        public static bool EsCoordenadaUtmValida(double? utmEste, double? utmNorte, List<string> errores)
        {
            if (!utmEste.HasValue || !utmNorte.HasValue)
            {
                errores.Add("Las coordenadas UTM est�n incompletas: faltan valores de Este o Norte.");
                return false;
            }

            if (double.IsNaN(utmEste.Value) || double.IsNaN(utmNorte.Value))
            {
                errores.Add("Las coordenadas UTM contienen valores no num�ricos.");
                return false;
            }

            if (utmEste.Value == 0 || utmNorte.Value == 0)
            {
                errores.Add("Las coordenadas UTM no pueden ser cero.");
                return false;
            }

            return true;
        }

        // Valida si las coordenadas geogr�ficas est�n en un rango aceptable
        public static bool ValidarCoordenadas(double latitud, double longitud, List<string> errores)
        {
            if (latitud < -90 || latitud > 90)
            {
                errores.Add($"La latitud '{latitud}' est� fuera del rango permitido (-90 a 90).");
                return false;
            }

            if (longitud < -180 || longitud > 180)
            {
                errores.Add($"La longitud '{longitud}' est� fuera del rango permitido (-180 a 180).");
                return false;
            }

            return true;
        }
    }
}
