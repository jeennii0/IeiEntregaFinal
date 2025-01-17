using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iei.Wrappers;
using Iei.Extractors;
using Iei.Repositories;
using Iei.Models.Dto;
using Iei.Models;

namespace Iei.Services
{
    public class CVService : ICVService
    {
        private readonly CVWrapper _wrapper;
        private readonly CVExtractor _extractor;
        private readonly ICVRepository _cvRepository;

        public CVService(CVWrapper wrapper, CVExtractor extractor, ICVRepository cvRepository)
        {
            _wrapper = wrapper;
            _extractor = extractor;
            _cvRepository = cvRepository;
        }

        public async Task<ResultadoExtraccionDto> ImportMonumentosAsync()
        {
            var monumentosCsv = _wrapper.ParseMonumentosCsv();

            var resultadoExtraccion = await _extractor.ExtractData(monumentosCsv);

            var listaNoDuplicados = new List<Monumento>();

            foreach (var monValido in resultadoExtraccion.MonumentosValidados)
            {
                var provincia = _cvRepository.GetOrCreateProvincia(monValido.Localidad.Provincia.Nombre);

                // Verificar o crear la localidad asociada a la provincia
                var localidad = _cvRepository.GetOrCreateLocalidad(monValido.Localidad.Nombre, provincia);

                // Asociar la localidad al monumento
                monValido.Localidad = localidad;
                if (_cvRepository.IsDuplicate(monValido))
                {
                    var motivo = "Monumento duplicado en la base de datos (mismo nombre y coordenadas)";
                    resultadoExtraccion.MonumentosRechazados.Add(new MonumentosRechazadosDto
                    {
                        Nombre = monValido.Nombre,
                        Localidad = monValido.Localidad?.Nombre ?? "",
                        MotivoError = motivo
                    });
                }
                else
                {
                    listaNoDuplicados.Add(monValido);
                }
            }

            if (listaNoDuplicados.Any())
            {
                foreach (var mon in listaNoDuplicados)
                {
                    _cvRepository.Add(mon);
                }
                await _cvRepository.SaveChangesAsync();
            }

            resultadoExtraccion.MonumentosInsertados = listaNoDuplicados.Count;

            return resultadoExtraccion;
        }
    }
}
