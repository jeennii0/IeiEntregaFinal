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
                errores.Add($"La descripción está vacía.");
                return false;
            }

            return true;
        }

        // Valida datos relacionados con la dirección y el código postal
        public static bool SonDatosDireccionValidos(string nombre, string codigoPostal, string direccion, string localidad, string provincia, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                errores.Add($"El código postal está vacío.");
                return false;
            }

            if (!EsCodigoPostalValido(codigoPostal, errores))
            {
                errores.Add($"El código postal '{codigoPostal}' no tiene un formato válido (5 dígitos).");
                return false;
            }

            if (string.IsNullOrWhiteSpace(provincia))
            {
                errores.Add($"La provincia está vacía.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(direccion))
            {
                errores.Add($"La dirección está vacía.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(localidad))
            {
                errores.Add($"La localidad está vacía.");
                return false;
            }

            return true;
        }

        // Completa un código postal de 4 dígitos con un '0' al inicio
        public static string CompletarCodigoPostal(string codigoPostal, List<string> correcciones)
        {
            codigoPostal = codigoPostal.Trim();
            if (codigoPostal.Length == 4)
            {
                correcciones.Add($"Código postal de 4 dígitos: se completó con '0'.");
                return "0" + codigoPostal;
            }
            return codigoPostal;
        }

        public static bool EsCodigoPostalValido(string codigoPostal, List<string> errores)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                errores.Add("El código postal está vacío o solo contiene espacios en blanco.");
                return false;
            }

            codigoPostal = codigoPostal.Trim();
            if (!Regex.IsMatch(codigoPostal, @"^\d{5}$"))
            {
                // Este método se encarga únicamente del formato (5 dígitos)
                errores.Add($"El código postal '{codigoPostal}' no tiene 5 dígitos.");
                return false;
            }

            return true;
        }

        // Verifica si un CP es correcto para una región (prefijos)
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
                errores.Add($"El código postal '{codigoPostal}' no corresponde a la región '{fuente}'.");
            }

            return esValido;
        }

        // Valida las coordenadas geográficas
        public static bool ValidarCoordenadas(double latitud, double longitud, List<string> errores)
        {

            if (latitud < -90 || latitud > 90)
            {
                errores.Add($"La latitud '{latitud}' está fuera del rango -90..90.");
                return false;
            }

            if (latitud ==0 || longitud == 0)
            {
                errores.Add($"La latitud o la longitud son 0");
                return false;
            }

            if (longitud < -180 || longitud > 180)
            {
                errores.Add($"La longitud '{longitud}' está fuera del rango -180..180.");
                return false;
            }

            return true;
        }
    }
}
