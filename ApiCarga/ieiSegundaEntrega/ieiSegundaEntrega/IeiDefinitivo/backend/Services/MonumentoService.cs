using Iei.Extractors;
using Iei.Modelos_Fuentes;
using Iei.Models;
using Iei.Models.Dto;
using Iei.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace Iei.Services
{
    public class MonumentoService
    {
        private readonly IeiContext _context;

        public MonumentoService(IeiContext context)
        {
            _context = context;
        }


        public async Task<(int cantidadInsertada, List<RegistroRechazadoDto> rechazados)> InsertarMonumento(List<Monumento> monumentos)
        {
            int monumentosInsertados = 0;
            var rechazados = new List<RegistroRechazadoDto>();

            try
            {
                foreach (var monumento in monumentos)
                {
                    // Verificar si ya existe un monumento con el mismo nombre
                    var monumentoExistente = await _context.Monumento
                        .FirstOrDefaultAsync(m => m.Nombre == monumento.Nombre);

                    if (monumentoExistente != null)
                    {
                        rechazados.Add(new RegistroRechazadoDto
                        {
                            Fuente = "Base de Datos",
                            Nombre = monumento.Nombre,
                            Localidad = monumento.Localidad.Nombre,
                            Motivo = "El monumento ya existe en la base de datos"
                        });
                        continue;
                    }

                    // Verificar si la localidad ya existe
                    var localidadExistente = await _context.Localidad
                        .FirstOrDefaultAsync(l => l.Nombre == monumento.Localidad.Nombre && l.Provincia.Nombre == monumento.Localidad.Provincia.Nombre);

                    if (localidadExistente != null)
                    {
                        monumento.LocalidadId = localidadExistente.Id;
                        monumento.Localidad = localidadExistente;
                    }
                    else
                    {
                        var provinciaExistente = await _context.Provincia
                            .FirstOrDefaultAsync(p => p.Nombre == monumento.Localidad.Provincia.Nombre);

                        if (provinciaExistente == null)
                        {
                            provinciaExistente = new Provincia { Nombre = monumento.Localidad.Provincia.Nombre };
                            _context.Provincia.Add(provinciaExistente);
                            await _context.SaveChangesAsync();
                        }

                        var nuevaLocalidad = new Localidad
                        {
                            Nombre = monumento.Localidad.Nombre,
                            ProvinciaId = provinciaExistente.Id,
                            Provincia = provinciaExistente
                        };

                        _context.Localidad.Add(nuevaLocalidad);
                        await _context.SaveChangesAsync();

                        monumento.LocalidadId = nuevaLocalidad.Id;
                        monumento.Localidad = nuevaLocalidad;
                    }

                    _context.Monumento.Add(monumento);
                    monumentosInsertados++;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar monumentos: {ex.Message}");
            }

            return (monumentosInsertados, rechazados);
        }




    }
}
