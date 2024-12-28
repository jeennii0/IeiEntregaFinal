using Iei.Models;

namespace Iei.Repositories
{
    public interface IEUSRepository
    {
        void Add(Monumento monumento);
        void AddRange(IEnumerable<Monumento> monumentos);
        bool IsDuplicate(Monumento mon);
        Task<Monumento> GetByIdAsync(int id);
        Task<int> SaveChangesAsync();
    }
}
