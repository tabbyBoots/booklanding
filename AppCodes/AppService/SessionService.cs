using System.Text.Json;

namespace mvcDapper3.AppCodes.AppService
{
    /// <summary>
    /// Session 類別
    /// </summary>
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// HttpContext 物件
        /// </summary>
        private HttpContext? _context { get { return _contextAccessor.HttpContext; } }

        public SessionService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }



        

        /// <summary>
        /// 設定 Session 物件值，使用 Json 序列化
        /// </summary>
        /// <typeparam name="T">物件類型</typeparam>
        /// <param name="key">Session 索引鍵</param>
        /// <param name="value">Session 值</param>
        /// <param name="expiry">過期時間（目前未使用）</param>
        /// <returns></returns>
        public Task SetObjectAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                string jsonValue = JsonSerializer.Serialize(value);
                _context?.Session.SetString(key, jsonValue);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 讀取 Session 物件值，使用 Json 反序列化
        /// </summary>
        /// <typeparam name="T">物件類型</typeparam>
        /// <param name="key">Session 索引鍵</param>
        /// <returns></returns>
        public Task<T?> GetObjectAsync<T>(string key)
        {
            try
            {
                var jsonValue = _context?.Session.GetString(key);
                if (string.IsNullOrEmpty(jsonValue))
                {
                    return Task.FromResult(default(T));
                }

                var result = JsonSerializer.Deserialize<T>(jsonValue);
                return Task.FromResult(result);
            }
            catch
            {
                return Task.FromResult(default(T));
            }
        }

        /// <summary>
        /// 移除 Session 值
        /// </summary>
        /// <param name="key">Session 索引鍵</param>
        /// <returns></returns>
        public Task RemoveAsync(string key)
        {
            try
            {
                _context?.Session.Remove(key);
                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        #region 額外的便利方法
        /// <summary>
        /// 讀取 Session 字串值，不使用 Json 反序列化
        /// </summary>
        /// <param name="keyName">Session 名稱</param>
        /// <param name="defaultValue">預設值</param>
        /// <returns></returns>
        public string GetSessionValue(string keyName, string defaultValue = "")
        {
            if (_context?.Session == null) return defaultValue;
            string str_value = _context.Session.GetString(keyName);
            return str_value ?? defaultValue;
        }

        /// <summary>
        /// 設定 Session 字串值，不使用 Json 序列化
        /// </summary>
        /// <param name="keyName">Session 名稱</param>
        /// <param name="value">Session 字串值</param>
        public void SetSessionValue(string keyName, string value)
        {
            _context?.Session.SetString(keyName, value);
        }

        /// <summary>
        /// 讀取 Session 字串值，使用 Json 反序列化
        /// </summary>
        /// <param name="keyName">Session 名稱</param>
        /// <returns></returns>
        public T GetSessionObjectValue<T>(string keyName)
        {
            var obj = (T)Activator.CreateInstance(typeof(T));
            string str_value = GetSessionValue(keyName);
            if (!string.IsNullOrEmpty(str_value))
                obj = JsonSerializer.Deserialize<T>(str_value);
            return obj;
        }

        /// <summary>
        /// 設定 Session 字串值，使用 Json 序列化
        /// </summary>
        /// <param name="keyName">Session 名稱</param>
        /// <param name="value">Session 字串值</param>
        public void SetSessionObjectValue<T>(string keyName, T value)
        {
            string str_value = JsonSerializer.Serialize(value);
            SetSessionValue(keyName, str_value);
        }
        #endregion
    }
}
