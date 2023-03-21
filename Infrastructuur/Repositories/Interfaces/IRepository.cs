using System.Linq.Expressions;

namespace Infrastructuur.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task DeleteAsync(Expression<Func<T, bool>> predicate);
        Task<T> UpdateAsync(Expression<Func<T, bool>> predicate, T entity);
        Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetAllAsync();
        Task DeleteRangeAsync(Expression<Func<T, bool>> predicate);

    }
}
