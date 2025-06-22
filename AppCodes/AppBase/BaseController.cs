using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mvcDapper3.AppCodes.AppInterface;

namespace mvcDapper3.AppCodes.AppBase;

public class BaseController : Controller
{
    #region
    protected IConfiguration config;
    #endregion

    #region "Session Service"
    private ISessionService? _service;

    /// <summary>
    /// 取得 SessionService 實例
    /// </summary>
    protected ISessionService SessionService
    {
        get
        {
            _service ??= HttpContext.RequestServices.GetRequiredService<ISessionService>();
            return _service;
        }
    }

    /// <summary>
    /// 讀取 Session 字串值，不使用 Json 反序列化
    /// </summary>
    /// <param name="keyName">Session 名稱</param>
    /// <param name="defaultValue">預設值</param>
    /// <returns></returns>
    protected string GetSessionValue(string keyName, string defaultValue = "")
    {
        try
        {
            var task = SessionService.GetObjectAsync<string>(keyName);
            task.Wait();
            return task.Result ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 設定 Session 字串值，不使用 Json 序列化
    /// </summary>
    /// <param name="keyName">Session 名稱</param>
    /// <param name="value">Session 字串值</param> 
    protected void SetSessionValue(string keyName, string value)
    {
        var task = SessionService.SetObjectAsync(keyName, value);
        task.Wait();
    }

    /// <summary>
    /// 讀取 Session 字串值，使用 Json 反序列化
    /// </summary>
    /// <param name="keyName">Session 名稱</param>
    /// <returns></returns>
    protected T GetSessionObjectValue<T>(string keyName)
    {
        var task = SessionService.GetObjectAsync<T>(keyName);
        task.Wait();
        return task.Result;
    }

    /// <summary>
    /// 設定 Session 字串值，使用 Json 序列化
    /// </summary>
    /// <param name="keyName">Session 名稱</param>
    /// <param name="value">Session 字串值</param>
    protected void SetSessionObjectValue<T>(string keyName, T value)
    {
        var task = SessionService.SetObjectAsync(keyName, value);
        task.Wait();
    }
    #endregion

}
