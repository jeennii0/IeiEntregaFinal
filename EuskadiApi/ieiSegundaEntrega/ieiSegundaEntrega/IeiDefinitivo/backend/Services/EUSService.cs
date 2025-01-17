using Iei.Models.Dto;
using Iei.Models;
using Iei.Repositories;
using Iei.Services;

public class EUSService : IEUSService
{
    private readonly EUSWrapper _wrapper;
    private readonly EUSExtractor _extractor;
    private readonly IEUSRepository _eusRepository;

    public EUSService(EUSWrapper wrapper, EUSExtractor extractor, IEUSRepository eusRepository)
    {
        _wrapper = wrapper;
        _extractor = extractor;
        _eusRepository = eusRepository;
    }

    public async Task<ResultadoExtraccionDto> ImportMonumentosAsync()
    {
        var monumentosJson = _wrapper.GenerateProcessedJson();
        var resultadoExtraccion = await _extractor.ExtractData(monumentosJson);

        var listaNoDuplicados = new List<Monumento>();

        foreach (var monValido in resultadoExtraccion.MonumentosValidados)
        {
            // Verificar o crear la provincia
            var provincia = _eusRepository.GetOrCreateProvincia(monValido.Localidad.Provincia.Nombre);

            // Verificar o crear la localidad asociada a la provincia
            var localidad = _eusRepository.GetOrCreateLocalidad(monValido.Localidad.Nombre, provincia);

            // Asociar la localidad al monumento
            monValido.Localidad = localidad;

            // Verificar si el monumento ya existe
            if (_eusRepository.IsDuplicate(monValido))
            {
                var motivo = "Monumento duplicado en la BD.";
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

        // Insertar los monumentos no duplicados en la BD
        if (listaNoDuplicados.Any())
        {
            foreach (var mon in listaNoDuplicados)
                _eusRepository.Add(mon);

            await _eusRepository.SaveChangesAsync();
        }

        resultadoExtraccion.MonumentosInsertados = listaNoDuplicados.Count;
        return resultadoExtraccion;
    }


}
