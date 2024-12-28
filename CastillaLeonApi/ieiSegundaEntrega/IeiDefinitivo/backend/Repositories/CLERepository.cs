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
