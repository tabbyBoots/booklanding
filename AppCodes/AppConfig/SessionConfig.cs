using System.ComponentModel.DataAnnotations;

namespace mvcDapper3.AppCodes.AppConfig;

/// <summary>
/// Session 設定類別 - 強型別設定
/// </summary>
public class SessionConfig
{
    public const string SectionName = "SessionSettings";
    
    /// <summary>
    /// Session 過期時間 (分鐘)
    /// </summary>
    [Range(1, 1440, ErrorMessage = "Session 過期時間必須在 1 到 1440 分鐘之間")]
    public int IdleTimeoutMinutes { get; set; } = 20;
    
    /// <summary>
    /// Cookie 名稱
    /// </summary>
    [Required(ErrorMessage = "Cookie 名稱不能為空")]
    public string CookieName { get; set; } = "mvcDapper9";
    
    /// <summary>
    /// 是否限制 HTTPS 連線
    /// </summary>
    public bool SecurePolicy { get; set; } = true;
    
    /// <summary>
    /// 是否限制伺服器端存取
    /// </summary>
    public bool HttpOnly { get; set; } = true;
    
    /// <summary>
    /// Cookie 網域
    /// </summary>
    public string? Domain { get; set; }
    
    /// <summary>
    /// Cookie 路徑
    /// </summary>
    public string Path { get; set; } = "/";
}
