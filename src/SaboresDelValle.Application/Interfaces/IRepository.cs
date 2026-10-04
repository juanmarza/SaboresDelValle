namespace SaboresDelValle.Application.Interfaces;

public interface IRepository<T>
{
    Task<int> InsertAsync(T entity);
    Task<int> UpdateAsync(T entity);
    Task<int> DeleteAsync(int id);
    Task<T?> SelectByIdAsync(int id);
    Task<List<T>> SelectAllAsync();
}
