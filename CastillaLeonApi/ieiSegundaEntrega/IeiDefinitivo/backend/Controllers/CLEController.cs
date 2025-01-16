using Microsoft.AspNetCore.Mvc;
using Iei.Services;
using Iei.Models.Dto;
using Iei.Models.Respuesta;

namespace Iei.Controllers
{
    [ApiController]
    [Route("api/castillaLeon")]
    public class CLEController : ControllerBase
    {
        private readonly ICLEService _service;

        public CLEController(ICLEService service)
        {
            _service = service;
        }
        /// <summary>
        /// Inserta los monumentos de Castilla y León en la base de datos.
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

                var response = new
                {
                    NumeroMonumentosInsertados = resultado.MonumentosInsertados,
                    MonumentoReparado = resultado.MonumentosReparados,
                    MonumentoDescartado = resultado.MonumentosRechazados
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error durante la importación: {ex.Message}");
            }
        }
    }
}
