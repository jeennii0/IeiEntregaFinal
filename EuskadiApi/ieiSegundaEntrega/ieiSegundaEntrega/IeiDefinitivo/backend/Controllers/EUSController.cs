using Microsoft.AspNetCore.Mvc;
using Iei.Services;
using Iei.Models.Dto;

namespace Iei.Controllers
{
    [ApiController]
    [Route("api/eus")]
    public class EusController : ControllerBase
    {
        private readonly IEUSService _service;

        public EusController(IEUSService service)
        {
            _service = service;
        }

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
