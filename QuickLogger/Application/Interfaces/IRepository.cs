namespace QuickLogger.Application.Interfaces;

public interface IRepository<T, TId>:IDisposable where T : class where TId : struct
{
    // 🔹 Crear
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

    // 🔹 Leer (Obtener un solo elemento)
    Task<T?> GetByIdAsync(TId id);
    Task<T?> GetSingleAsync(Func<T, bool> predicate);

    // 🔹 Leer (Obtener múltiples elementos)
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate);
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task<IEnumerable<T>> GetSortedAsync<TKey>(Func<T, TKey> keySelector, bool ascending = true, int pageNumber=1, int pageSize=10);

    // 🔹 Actualizar
    Task<bool> UpdateAsync(T entity);
    Task<bool> UpdateRangeAsync(IEnumerable<T> entities);

    // 🔹 Eliminar
    Task<bool> DeleteAsync(T entity);
    Task<bool> DeleteByIdAsync(TId id);
    Task<bool> DeleteRangeAsync(IEnumerable<T> entities);

    // 🔹 Contadores
    Task<int> CountAsync();
    Task<int> CountAsync(Func<T, bool> predicate);

    // 🔹 Existencias
    Task<bool> ExistsAsync(TId id);
    Task<bool> ExistsAsync(Func<T, bool> predicate);

    // 🔹 Búsquedas avanzadas
    Task<IEnumerable<T>> SearchAsync(Func<T, bool> predicate);
    Task<IEnumerable<T>> SearchWithPagingAsync(Func<T, bool> predicate, int pageNumber, int pageSize);

    // 🔹 Transacciones
    Task<IDatabaseTransaction> BeginTransactionAsyn();

}
