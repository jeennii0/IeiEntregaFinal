using Iei.Wrappers;
using Iei.Extractors;
using Iei.Repositories;
using Iei.Models.Dto;
using Iei.Models;

namespace Iei.Services
{

    public class CLEService : ICLEService
    {
        private readonly CLEWrapper _wrapper;
        private readonly CLEExtractor _extractor;
        private readonly ICLERepository _cleRepository;

        public CLEService(CLEWrapper wrapper, CLEExtractor extractor, ICLERepository cleRepository)
        {
            _wrapper = wrapper;
            _extractor = extractor;
            _cleRepository = cleRepository;
        }

        public async Task<ResultadoExtraccionDto> ImportMonumentosAsync()
        {
            var monumentosXml = _wrapper.ConvertXmlToJson();
            var resultadoExtraccion = await _extractor.ExtractData(monumentosXml);
            var listaNoDuplicados = new List<Monumento>();
            var loteDuplicados = new HashSet<(string, double, double)>();
     
            foreach (var monValido in resultadoExtraccion.MonumentosValidados)
            {
                var provincia = _cleRepository.GetOrCreateProvincia(monValido.Localidad.Provincia.Nombre);

                // Verificar o crear la localidad asociada a la provincia
                var localidad = _cleRepository.GetOrCreateLocalidad(monValido.Localidad.Nombre, provincia);

                // Asociar la localidad al monumento
                monValido.Localidad = localidad;
                var clave = (monValido.Nombre, monValido.Latitud, monValido.Longitud);

                if (loteDuplicados.Contains(clave))
                {
                    var motivo = "Monumento duplicado en el mismo lote (mismo nombre y coordenadas)";
                    resultadoExtraccion.MonumentosRechazados.Add(new MonumentosRechazadosDto
                    {
                        Nombre = monValido.Nombre,
                        Localidad = monValido.Localidad?.Nombre ?? "",
                        MotivoError = motivo
                    });
                }
                else
                {
                    loteDuplicados.Add(clave);
                    listaNoDuplicados.Add(monValido);
                }
            }

            var listaParaInsertar = new List<Monumento>();
            foreach (var monVal in listaNoDuplicados)
            {
                if (_cleRepository.IsDuplicate(monVal))
                {
                
                    var motivo = "Monumento duplicado en la BD (mismo nombre y coordenadas)";
                    resultadoExtraccion.MonumentosRechazados.Add(new MonumentosRechazadosDto
                    {
                        Nombre = monVal.Nombre,
                        Localidad = monVal.Localidad?.Nombre ?? "",
                        MotivoError = motivo
                    });
                }
                else
                {
                    listaParaInsertar.Add(monVal);
                }
            }

            
            if (listaParaInsertar.Any())
            {
                foreach (var mon in listaParaInsertar)
                    _cleRepository.Add(mon);

                await _cleRepository.SaveChangesAsync();
            }


            int duplicados = resultadoExtraccion.MonumentosValidados.Count - listaParaInsertar.Count;
            resultadoExtraccion.MonumentosInsertados -= duplicados;

            return resultadoExtraccion;
        }

    }
}
