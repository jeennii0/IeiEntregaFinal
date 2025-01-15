using Microsoft.AspNetCore.Mvc;
using Iei.Models;
using Iei.Services;

[ApiController]
[Route("api/[controller]")]
public class CargarDatosController : ControllerBase
{
    private readonly ICargaService _cargaService;

    public CargarDatosController(ICargaService cargaService)
    {
        _cargaService = cargaService;
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportData([FromBody] CargaRequest request)
    {
        var resultadoGlobal = await _cargaService.ImportDataAsync(request);

        if (resultadoGlobal.Count == 0)
        {
            return BadRequest("No se solicitó la carga de ninguna comunidad.");
        }

        return Ok(resultadoGlobal);
    }
}