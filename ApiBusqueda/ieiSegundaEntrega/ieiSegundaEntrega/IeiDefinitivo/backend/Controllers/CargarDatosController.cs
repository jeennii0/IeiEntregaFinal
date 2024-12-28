using Iei.Extractors;
using Iei.Extractors.ValidacionMonumentos;
using Iei.Modelos_Fuentes;
using Iei.ModelosFuentesOriginales;
using Iei.Models;
using Iei.Models.Dto;
using Iei.Services;
using Iei.Wrappers;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Iei.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CargarDatosController:ControllerBase
    {
        private readonly MonumentoService _monumentoService;

        private readonly CLEService _cleService;
        private readonly CVService  _cvService;
        private readonly EUSService _eusService;

        private readonly CLEExtractor _cleExtractor;
        private readonly CVExtractor  _cvExtractor;
        private readonly EUSExtractor _eusExtractor;

        public CargarDatosController(MonumentoService monumentoService, CLEService cleService,
                                    CVService cvService, EUSService eusService, CLEExtractor cleExtractor,
                                    CVExtractor cvExtractor, EUSExtractor eusExtractor)
        {
            _monumentoService = monumentoService;
            _cleService = cleService;
            _cvService = cvService;
            _eusService = eusService;
            _cleExtractor = cleExtractor;
            _cvExtractor = cvExtractor;
            _eusExtractor = eusExtractor;
        }
        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarMonumentos([FromBody] RegionDto regionDto)
        {
            if (regionDto.Regiones == null || regionDto.Regiones.Count == 0)
            {
                return BadRequest("Debe seleccionar al menos una región.");
            }

            // DTO para almacenar resultados
            var resultado = new ResultadoCargaDto
            {
                NumeroRegistrosCorrectos = 0,
                RegistrosCorregidos = new List<RegistroCorregidoDto>(),
                RegistrosRechazados = new List<RegistroRechazadoDto>()
            };

            var monumentosAInsertar = new List<Monumento>();
            var errores = new List<string>();
            var correcciones = new List<string>();

            try
            {
                foreach (var region in regionDto.Regiones)
                {
                    IEnumerable<Monumento> monumentos = null;

                    switch (region)
                    {
                        case "Castilla y León":
                            monumentos = await _cleExtractor.ExtractData(_cleService.ObtenerDatosXmlCastillaLeon());
                            break;
                        case "Euskadi":
                            monumentos = await _eusExtractor.ExtractDataAsync(_eusService.ObtenerDatosJsonEuskadi());
                            break;
                        case "Comunidad Valenciana":
                            monumentos = await _cvExtractor.ExtractData(_cvService.ObtenerDatosCsvCV());
                            break;
                        default:
                            return BadRequest($"Región no soportada: {region}");
                    }

                    foreach (var monumento in monumentos)
                    {
                        // Validación inicial
                        if (!ValidacionesMonumentos.EsMonumentoInicialValido(monumento.Nombre, monumento.Descripcion, errores))
                        {
                            resultado.RegistrosRechazados.Add(new RegistroRechazadoDto
                            {
                                Fuente = region,
                                Nombre = monumento.Nombre ?? "Desconocido",
                                Localidad = monumento.Localidad?.Nombre ?? "Desconocida",
                                Motivo = errores[^1]
                            });
                            continue;
                        }

                        // Validación de coordenadas
                        if (!ValidacionesMonumentos.ValidarCoordenadas(monumento.Latitud, monumento.Longitud, errores))
                        {
                            resultado.RegistrosRechazados.Add(new RegistroRechazadoDto
                            {
                                Fuente = region,
                                Nombre = monumento.Nombre ?? "Desconocido",
                                Localidad = monumento.Localidad?.Nombre ?? "Desconocida",
                                Motivo = errores[^1]
                            });
                            continue;
                        }

                        // Correcciones
                        var corregido = false;
                        if (monumento.CodigoPostal.Length == 4)
                        {
                            monumento.CodigoPostal = ValidacionesMonumentos.CompletarCodigoPostal(monumento.CodigoPostal, correcciones);
                            corregido = true;
                        }

                        if (corregido)
                        {
                            resultado.RegistrosCorregidos.Add(new RegistroCorregidoDto
                            {
                                Fuente = region,
                                Nombre = monumento.Nombre ?? "Desconocido",
                                Localidad = monumento.Localidad?.Nombre ?? "Desconocida",
                                Motivo = correcciones[^1],
                                Operacion = "Insertado"
                            });
                        }

                        // Incrementar los registros correctos solo si el monumento pasa todas las validaciones
                        resultado.NumeroRegistrosCorrectos++;
                        monumentosAInsertar.Add(monumento);
                    }
                }

                // Insertar en la base de datos
                var (cantidadInsertada, rechazadosEnBD) = await _monumentoService.InsertarMonumento(monumentosAInsertar);

                // Actualizar los rechazados en el DTO con los obtenidos de la base de datos
                resultado.RegistrosRechazados.AddRange(rechazadosEnBD);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al procesar la solicitud: {ex.Message}");
            }
        }



        [HttpGet("")]
        public async Task<IActionResult> ConvertirYInsertarDatos([FromQuery] string source)
        {
            try
            {
                List<Monumento> monumentos = new List<Monumento>();

                // Dependiendo de la fuente, obtenemos los monumentos
                if (source.Equals("CLE", StringComparison.OrdinalIgnoreCase))
                {
                    CLEWrapper xmlWrapper = new CLEWrapper();
                    List<ModeloXMLOriginal> xmlData = xmlWrapper.ConvertXmlToJson();
                    CLEExtractor extractorXml = new CLEExtractor();
                    monumentos = await extractorXml.ExtractData(xmlData);
                }
                else if (source.Equals("EUS", StringComparison.OrdinalIgnoreCase))
                {
                    EUSWrapper jsonWrapper = new EUSWrapper();
                    List<ModeloJSONOriginal> jsonEus = jsonWrapper.GenerateProcessedJson();
                    EUSExtractor extractorJson = new EUSExtractor();
                    monumentos = await extractorJson.ExtractDataAsync(jsonEus);
                }
                else if (source.Equals("CV", StringComparison.OrdinalIgnoreCase))
                {
                    CVWrapper csvWrapper = new CVWrapper();
                    List<ModeloCSVOriginal> csvData = csvWrapper.ParseMonumentosCsv();
                    CVExtractor extractorCsv = new CVExtractor();
                    monumentos = await extractorCsv.ExtractData(csvData);
                }
                else
                {
                    return BadRequest("Parámetro 'source' no válido. Use 'xml', 'json' o 'csv'.");
                }

                // Llamada al método InsertarMonumento, que ahora devuelve una tupla
                var (cantidadInsertada, rechazados) = await _monumentoService.InsertarMonumento(monumentos);

                // Actualiza los datos del DTO para reflejar los monumentos rechazados
                //resultado.RegistrosRechazados.AddRange(rechazados);

                // Retorna un mensaje exitoso con la cantidad insertada
                return Ok(new { message = $"{cantidadInsertada} monumentos insertados correctamente." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

    }
        }
    



