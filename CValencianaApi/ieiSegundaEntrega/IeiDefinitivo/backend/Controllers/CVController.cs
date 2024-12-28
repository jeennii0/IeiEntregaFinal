using Microsoft.AspNetCore.Mvc;
using Iei.Services;

namespace Iei.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CVController : ControllerBase
    {
        private readonly ICVService _service;

        public CVController(ICVService service)
        {
            _service = service;
        }

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
