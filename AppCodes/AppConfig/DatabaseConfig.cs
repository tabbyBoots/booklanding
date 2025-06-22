
namespace mvcDapper3.AppCodes.AppConfig;

/// <summary>
/// 資料庫連線設定類別 - 強型別設定
/// </summary>
public class DatabaseConfig
{
    public const string SectionName = "ConnectionStrings";
    
    /// <summary>
    /// 主要資料庫連線字串
    /// </summary>
    [Required(ErrorMessage = "主要資料庫連線字串不能為空")]
    public string DbConn { get; set; } = string.Empty;
    
    /// <summary>
    /// 次要資料庫連線字串
    /// </summary>
    [Required(ErrorMessage = "次要資料庫連線字串不能為空")]
    public string DbConn2 { get; set; } = string.Empty;
    
    /// <summary>
    /// 第三個資料庫連線字串
    /// </summary>
    public string? DbConn3 { get; set; }
    
    /// <summary>
    /// 正式環境資料庫連線字串
    /// </summary>
    public string? ProductionDbConn { get; set; }
    
    /// <summary>
    /// 取得根據環境選擇的連線字串，這個是直接使用連線字串，不存取系統變數
    /// </summary>
    /// <param name="isProduction">是否為正式環境</param>
    /// <returns>適當的連線字串</returns>
    public string GetConnectionString(bool isProduction = false)
    {
        return isProduction && !string.IsNullOrEmpty(ProductionDbConn) 
            ? ProductionDbConn 
            : DbConn;
    }
    
    /// <summary>
    /// 取得安全連線字串（優先使用環境變數），使用系統變數取得連線字串
    /// </summary>
    /// <param name="isProduction">是否為正式環境</param>
    /// <returns>連線字串</returns>
    public string GetSecureConnectionString(bool isProduction = false)
    {
        // 從環境變數取得資料庫憑證
        var server = Environment.GetEnvironmentVariable("DB_SERVER");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        
        // 如果所有環境變數都存在，使用它們建立連線字串
        if (!string.IsNullOrEmpty(server) && 
            !string.IsNullOrEmpty(dbName) &&
            !string.IsNullOrEmpty(user) &&
            !string.IsNullOrEmpty(password))
        {
            return $"Server={server};Database={dbName};User Id={user};Password={password};TrustServerCertificate=True;Connection Timeout=120";
        }
        
        // 回退到標準連線字串
        return GetConnectionString(isProduction);
    }
}
