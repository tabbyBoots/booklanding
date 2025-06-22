
namespace mvcDapper3.AppCodes.AppInterface;

/// <summary>
/// BaseRepo介面，定義CRUD泛型方法
/// 不包含連線設定，這個在BaseRepo做
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IBaseRepo<TEntity, TKey>
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(TKey key);
    Task AddAsync(TEntity entity);
    Task EditAsync(TEntity entity);
    Task DeleteAsync(TKey key);
}
