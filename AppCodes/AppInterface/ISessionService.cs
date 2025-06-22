
namespace mvcDapper3.AppCodes.AppInterface
{
    public interface ISessionService
    {
        Task SetObjectAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetObjectAsync<T>(string key);
        Task RemoveAsync(string key);
    }
}
