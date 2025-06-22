
namespace mvcDapper3.AppCodes.AppBase;

public abstract class BaseRepo<TEntity, TKey> : IBaseRepo<TEntity, TKey>
{

    protected readonly IConnFactory _connFactory;
    protected readonly string _tbName;
    protected Dictionary<string, object> _queryParams = new();

    /// <summary>
    /// 建構子
    /// 設定連線字串及資料表名稱
    /// </summary>
    /// <param name="connFactory"></param>
    /// <param name="tbName"></param>
    public BaseRepo(IConnFactory connFactory, string tbName)
    {
        _connFactory = connFactory;
        _tbName = tbName;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        try
        {
            // 切換語系 - 處理時間有'上午'等地區語言
            var orgCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("zh-TW");

            var query = BuildSelectQuery();
            using (var _conn = _connFactory.GetOpenConn())
            {
                var result = await _conn.QueryAsync<TEntity>(query, _queryParams);

                // 還原語系
                CultureInfo.CurrentCulture = orgCulture;

                return result;
            } 
        }
        catch (Exception e)
        {
            throw new Exception("Failed! Message: " + e.Message + "\r\n" + e.StackTrace.ToString());
        }
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        try
        {
            var query = BuildInsertQuery();
            using (var _conn = _connFactory.GetOpenConn())
            {
                var result = await _conn.ExecuteAsync(query, entity);
                // result<=0 throw ...
            }
        }
        catch (Exception e)
        {
            throw new Exception("Failed! Message: " + e.Message + "\r\n" + e.StackTrace.ToString());
        }
    }

    public virtual async Task DeleteAsync(TKey key)
    {
        try
        {
            using (var _conn = _connFactory.GetOpenConn())
            {
                await _conn.ExecuteAsync(
                $"DELETE FROM {_tbName} WHERE Id = @Id", new { Id = key });
            }
        }
        catch (Exception e)
        {
            throw new Exception("Operation failed", e);
        }
    }

    public virtual async Task EditAsync(TEntity entity)
    {
        try
        {
            var query = BuildUpdateQuery();

            using (var _conn = _connFactory.GetOpenConn())
            {
                var result = await _conn.ExecuteAsync(query, entity);
                if (result <= 0) throw new Exception("Operation failed");
            } 
        }
        catch (Exception e)
        {
            throw new Exception("Operation failed", e);
        }
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey key)
    {
        try
        {
            using (var _conn = _connFactory.GetOpenConn())
            {
                // 切換語系 - 處理時間有'上午'等地區語言
                var orgCulture = CultureInfo.CurrentCulture;
                CultureInfo.CurrentCulture = new CultureInfo("zh-TW");

                var query = BuildSelectQuery();
                var result = await _conn.QueryFirstOrDefaultAsync<TEntity>(query, _queryParams);

                // 還原語系
                CultureInfo.CurrentCulture = orgCulture;
                return result;
            }
        }
        catch (Exception e)
        {
            throw new Exception("Failed", e);
        }
    }

    //建立 Incert 查詢程式字串
    protected abstract string BuildInsertQuery();
    //建立 Update 查詢程式字串
    protected abstract string BuildUpdateQuery();
    //建立 Select 查詢程式字串
    protected abstract string BuildSelectQuery();
}
