using Iei.Models;
using Microsoft.EntityFrameworkCore;
using static Iei.Repositories.CLERepository;

namespace Iei.Repositories
{
    public class CLERepository : ICLERepository
    {
            private readonly IeiContext _ieiContext;

            public CLERepository(IeiContext ieiContext)
            {
            _ieiContext = ieiContext;
            }
        public Provincia GetOrCreateProvincia(string nombre)
        {
            var provincia = _ieiContext.Provincia
            .FirstOrDefault(p => p.Nombre.ToLower() == nombre.ToLower());

            if (provincia == null)
            {
                provincia = new Provincia { Nombre = nombre };
                _ieiContext.Provincia.Add(provincia);
                _ieiContext.SaveChanges(); // Guardar cambios para obtener el ID
            }

            return provincia;
        }
        public Localidad GetOrCreateLocalidad(string nombre, Provincia provincia)
        {
            var localidad = _ieiContext.Localidad
        .FirstOrDefault(l =>
            l.Nombre.ToLower() == nombre.ToLower() &&
            l.ProvinciaId == provincia.Id);
            if (localidad == null)
            {
                localidad = new Localidad { Nombre = nombre, Provincia = provincia };
                _ieiContext.Localidad.Add(localidad);
                _ieiContext.SaveChanges(); // Guardar cambios para obtener el ID
            }

            return localidad;
        }

        public bool IsDuplicate(Monumento mon)
            {
                return _ieiContext.Monumento.Any(m =>
                    m.Nombre == mon.Nombre &&
                    m.Latitud == mon.Latitud &&
                    m.Longitud == mon.Longitud
                );
            }
        public void Add(Monumento monumento)
            {
            _ieiContext.Monumento.Add(monumento);
            }

            public void AddRange(IEnumerable<Monumento> monumentos)
            {
            _ieiContext.Monumento.AddRange(monumentos);
            }

            public async Task<Monumento> GetByIdAsync(int id)
            {
                return await _ieiContext.Monumento.FindAsync(id);
            }

            public async Task<int> SaveChangesAsync()
            {
                return await _ieiContext.SaveChangesAsync();
            }
        }
}
