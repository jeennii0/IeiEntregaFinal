using System.Text.RegularExpressions;

namespace Iei.Extractors.ValidacionMonumentos
{
    public static class ValidacionesMonumentos
    {
        // Valida los datos iniciales del monumento
        public static bool EsMonumentoInicialValido(string nombre, string descripcion, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                errores.Add("El monumento no tiene nombre.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                errores.Add($"La descripci�n est� vac�a.");
                return false;
            }

            return true;
        }

        // Valida datos relacionados con la direcci�n y el c�digo postal
        public static bool SonDatosDireccionValidos(string nombre, string codigoPostal, string direccion, string localidad, string provincia, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                errores.Add($"El c�digo postal est� vac�o.");
                return false;
            }

            if (!EsCodigoPostalValido(codigoPostal, errores))
            {
                errores.Add($"El c�digo postal '{codigoPostal}' no tiene un formato v�lido (5 d�gitos).");
                return false;
            }

            if (string.IsNullOrWhiteSpace(provincia))
            {
                errores.Add($"La provincia est� vac�a.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(direccion))
            {
                errores.Add($"La direcci�n est� vac�a.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(localidad))
            {
                errores.Add($"La localidad est� vac�a.");
                return false;
            }

            return true;
        }

        // Completa un c�digo postal de 4 d�gitos con un '0' al inicio
        public static string CompletarCodigoPostal(string codigoPostal, List<string> correcciones)
        {
            codigoPostal = codigoPostal.Trim();
            if (codigoPostal.Length == 4)
            {
                correcciones.Add($"C�digo postal de 4 d�gitos: se complet� con '0'.");
                return "0" + codigoPostal;
            }
            return codigoPostal;
        }

        public static bool EsCodigoPostalValido(string codigoPostal, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                errores.Add("El c�digo postal est� vac�o o solo contiene espacios en blanco.");
                return false;
            }

            codigoPostal = codigoPostal.Trim();
            if (!Regex.IsMatch(codigoPostal, @"^\d{5}$"))
            {
                // Este m�todo se encarga �nicamente del formato (5 d�gitos)
                errores.Add($"El c�digo postal '{codigoPostal}' no tiene 5 d�gitos.");
                return false;
            }

            return true;
        }

        // Verifica si un CP es correcto para una regi�n (prefijos)
        public static bool EsCodigoPostalCorrectoParaRegion(string codigoPostal, string fuente, List<string> errores)
        {
            bool esValido = fuente switch
            {
                "CV" => codigoPostal.StartsWith("03")
                     || codigoPostal.StartsWith("12")
                     || codigoPostal.StartsWith("46"),

                "CLE" => codigoPostal.StartsWith("05")
                      || codigoPostal.StartsWith("09")
                      || codigoPostal.StartsWith("24")
                      || codigoPostal.StartsWith("34")
                      || codigoPostal.StartsWith("37")
                      || codigoPostal.StartsWith("40")
                      || codigoPostal.StartsWith("42")
                      || codigoPostal.StartsWith("47")
                      || codigoPostal.StartsWith("49"),

                "EUS" => codigoPostal.StartsWith("01")
                      || codigoPostal.StartsWith("20")
                      || codigoPostal.StartsWith("48"),

                _ => true
            };

            if (!esValido)
            {
                errores.Add($"El c�digo postal '{codigoPostal}' no corresponde a la regi�n '{fuente}'.");
            }

            return esValido;
        }

        // Valida las coordenadas geogr�ficas
        public static bool ValidarCoordenadas(double latitud, double longitud, List<string> errores)
        {

            if (latitud < -90 || latitud > 90)
            {
                errores.Add($"La latitud '{latitud}' est� fuera del rango -90..90.");
                return false;
            }

            if (latitud ==0 || longitud == 0)
            {
                errores.Add($"La latitud o la longitud son 0");
                return false;
            }

            if (longitud < -180 || longitud > 180)
            {
                errores.Add($"La longitud '{longitud}' est� fuera del rango -180..180.");
                return false;
            }

            return true;
        }
    }
}
