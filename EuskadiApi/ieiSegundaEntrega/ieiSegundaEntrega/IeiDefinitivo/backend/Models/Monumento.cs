﻿namespace Iei.Models
{
    public class Monumento : Base<int>
    {
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public double Longitud { get; set; }
        public double Latitud { get; set; }
        public int LocalidadId { get; set; }
        public Localidad? Localidad { get; set; }

    }
}
