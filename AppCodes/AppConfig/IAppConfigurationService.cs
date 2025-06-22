using Microsoft.Extensions.Options;

namespace mvcDapper3.AppCodes.AppConfig;

/// <summary>
/// 型別安全設定存取服務介面
/// </summary>
public interface IAppConfigurationService
{
    /// <summary>
    /// 取得資料庫設定
    /// </summary>
    DatabaseConfig Database { get; }
    
    /// <summary>
    /// 取得 JWT 設定
    /// </summary>
    JwtConfig Jwt { get; }
    
    /// <summary>
    /// 取得 Session 設定
    /// </summary>
    SessionConfig Session { get; }
    
    /// <summary>
    /// 取得應用程式設定
    /// </summary>
    AppConfig App { get; }
    
    /// <summary>
    /// 重新載入所有設定
    /// </summary>
    void ReloadConfigurations();
    
    /// <summary>
    /// 驗證所有設定的正確性
    /// </summary>
    /// <returns>驗證結果清單</returns>
    IEnumerable<string> ValidateAllConfigurations();
}
