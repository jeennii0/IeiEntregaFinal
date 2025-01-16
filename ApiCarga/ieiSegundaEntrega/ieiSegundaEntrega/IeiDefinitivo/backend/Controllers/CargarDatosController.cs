using Microsoft.AspNetCore.Mvc;
using Iei.Models;
using Iei.Services;

[ApiController]
[Route("api/carga")]
public class CargarDatosController : ControllerBase
{
    private readonly ICargaService _cargaService;

    public CargarDatosController(ICargaService cargaService)
    {
        _cargaService = cargaService;
    }
    /// <summary>
    /// Importa datos de las distintas fuentes, según las que se envíen en el parámetro.
    /// </summary>
    /// <param name="request">Objeto que indica qué fuentes o comunidades se quieren importar.</param>
    /// <returns>
    /// Devuelve un listado de resultados de la carga, o un BadRequest si no se solicitó ninguna comunidad.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoMicroservicio))]
    public async Task<IActionResult> ImportData([FromBody] CargaRequest request)
    {
        var resultadoGlobal = await _cargaService.ImportDataAsync(request);

        if (resultadoGlobal.Count == 0)
        {
            return BadRequest("No se solicitó la carga de ninguna comunidad.");
        }

        return Ok(resultadoGlobal);
    }
    /// <summary>
    /// Borra el almacén de datos (vacía la base de datos).
    /// </summary>
    /// <returns>Ok si se vació exitosamente, o un StatusCode(500) si ocurrió un error.</returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteAllData()
    {
        try
        {
            await _cargaService.VaciarBaseDeDatosAsync();
            return Ok("Base de datos vaciada exitosamente.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al vaciar la base de datos: {ex.Message}");
        }
    }


}