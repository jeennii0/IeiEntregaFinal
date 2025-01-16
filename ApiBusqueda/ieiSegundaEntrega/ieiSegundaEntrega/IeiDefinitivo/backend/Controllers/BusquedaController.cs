using Iei.Extractors;
using Iei.Extractors.ValidacionMonumentos;
using Iei.Modelos_Fuentes;
using Iei.ModelosFuentesOriginales;
using Iei.Models;
using Iei.Models.Dto;
using Iei.Services;
using Iei.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;

namespace Iei.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusquedaController : ControllerBase
    {
        private readonly MonumentoService _monumentoService;
        private readonly IeiContext _context;

        public BusquedaController(IeiContext context)
        {
            _context = context;
        }

        [HttpGet("buscarMonumentos")]
        public async Task<IActionResult> BuscarMonumentos(
            [FromQuery] string? Localidad,
            [FromQuery] string? CodigoPostal,
            [FromQuery] string? Provincia,
            [FromQuery] string? Tipo)
        {
            try
            {
                var monumento = _context.Monumento.AsQueryable();

                // Filtro por localidad, insensible a mayúsculas/minúsculas
                if (!string.IsNullOrEmpty(Localidad))
                {
                    var localidadNormalizada = Localidad.ToLower();
                    monumento = monumento.Where(i => i.Localidad.Nombre.ToLower().Contains(localidadNormalizada));
                }

                // Filtro por código postal, insensible a mayúsculas/minúsculas
                if (!string.IsNullOrEmpty(CodigoPostal))
                {
                    var codigoPostalNormalizado = CodigoPostal.ToLower();
                    monumento = monumento.Where(i => i.CodigoPostal.ToLower() == codigoPostalNormalizado);
                }

                // Filtro por provincia, insensible a mayúsculas/minúsculas
                if (!string.IsNullOrEmpty(Provincia))
                {
                    var provinciaNormalizada = Provincia.ToLower();
                    monumento = monumento.Where(i => i.Localidad.Provincia.Nombre.ToLower() == provinciaNormalizada);
                }

                // Filtro por tipo, insensible a mayúsculas/minúsculas
                if (!string.IsNullOrEmpty(Tipo))
                {
                    var tipoNormalizado = Tipo.ToLower();
                    monumento = monumento.Where(i => i.Tipo.ToLower() == tipoNormalizado);
                }

                var resultados = await monumento
                .Select(i => new BusquedaDTO
                    {
                        Nombre = i.Nombre,
                        Tipo = i.Tipo, 
                        Direccion = i.Direccion,
                        Localidad = i.Localidad.Nombre,
                        CodigoPostal = i.CodigoPostal,
                        Provincia = i.Localidad.Provincia.Nombre,
                        Descripcion = i.Descripcion
                    })
                 .ToListAsync();

                return Ok(resultados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error durante la búsqueda: {ex.Message}");
            }
        }

        [HttpGet("obtenerMonumentos")]
        public async Task<IActionResult> ObtenerMonumentos()
        {
            try
            {
                // Obtén todos los monumentos de la base de datos
                var monumentos = await _context.Monumento
                    .Select(i => new MonumentosMapaDTO
                    {
                        Nombre = i.Nombre,
                        Tipo = i.Tipo,
                        Direccion = i.Direccion,
                        Localidad = i.Localidad.Nombre,
                        CodigoPostal = i.CodigoPostal,
                        Provincia = i.Localidad.Provincia.Nombre,
                        Descripcion = i.Descripcion,
                        Longitud = i.Longitud, 
                        Latitud = i.Latitud
                    })
                .ToListAsync();

                // Devuelve los datos en la respuesta
                return Ok(monumentos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los monumentos: {ex.Message}");
            }
        }

    }
}
