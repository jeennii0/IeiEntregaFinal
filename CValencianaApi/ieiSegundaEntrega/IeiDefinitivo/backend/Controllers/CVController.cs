using Microsoft.AspNetCore.Mvc;
using Iei.Services;
using Iei.Models.Dto;

namespace Iei.Controllers
{
    [ApiController]
    [Route("api/comunidadValenciana")]
    public class CVController : ControllerBase
    {
        private readonly ICVService _service;

        public CVController(ICVService service)
        {
            _service = service;
        }
        /// <summary>
        /// Inserta los monumentos de la Comunidad Valenciana en la base de datos.
        /// </summary>
        /// <remarks>
        /// Este método procesa una lista de monumentos, insertando los válidos en la base de datos.
        /// Los monumentos inválidos (por ejemplo, sin código postal o coordenadas) son descartados.
        /// </remarks>
        /// <returns>
        /// Devuelve un resumen con el número de monumentos insertados, reparados y descartados.
        /// En caso de error, devuelve un código de estado 500 con un mensaje de error detallado.
        /// </returns>
        /// <response code="200">El resumen de la operación con detalles de monumentos procesados.</response>
        /// <response code="500">Error interno durante la importación.</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response))]
        [HttpPost("import")]
        public async Task<IActionResult> ImportMonumentos()
        {
            try
            {
                var resultado = await _service.ImportMonumentosAsync();

                return Ok(new
                {
                    NumeroMonumentosInsertados = resultado.MonumentosInsertados,
                    MonumentoReparado = resultado.MonumentosReparados,
                    MonumentoDescartado = resultado.MonumentosRechazados

                });
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(500, $"Error durante la importación: {ex.Message}");
            }
        }
    }
}
