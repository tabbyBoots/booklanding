using System.ComponentModel.DataAnnotations;

namespace mvcDapper3.AppCodes.AppConfig;

/// <summary>
/// JWT 驗證設定類別 - 強型別設定
/// </summary>
public class JwtConfig
{
    public const string SectionName = "JwtSettings";
    
    /// <summary>
    /// JWT 發行者
    /// </summary>
    [Required(ErrorMessage = "JWT 發行者不能為空")]
    public string Issuer { get; set; } = "mvcDapper9";
    
    /// <summary>
    /// JWT 接收者
    /// </summary>
    [Required(ErrorMessage = "JWT 接收者不能為空")]
    public string Audience { get; set; } = "mvcDapper9";
    
    /// <summary>
    /// JWT 簽章金鑰 (應從環境變數或安全儲存中取得)
    /// </summary>
    [Required(ErrorMessage = "JWT 簽章金鑰不能為空")]
    [MinLength(32, ErrorMessage = "JWT 簽章金鑰長度至少需要 32 個字元")]
    public string SignKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Token 過期時間 (分鐘)
    /// </summary>
    [Range(1, 1440, ErrorMessage = "Token 過期時間必須在 1 到 1440 分鐘之間")]
    public int ExpirationMinutes { get; set; } = 60;
    
    /// <summary>
    /// 時間偏移量 (秒)
    /// </summary>
    [Range(0, 300, ErrorMessage = "時間偏移量必須在 0 到 300 秒之間")]
    public int ClockSkewSeconds { get; set; } = 30;
    
    /// <summary>
    /// 是否驗證發行者
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;
    
    /// <summary>
    /// 是否驗證接收者
    /// </summary>
    public bool ValidateAudience { get; set; } = false;
    
    /// <summary>
    /// 是否驗證生命週期
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;
    
    /// <summary>
    /// 是否驗證簽章金鑰
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;
}
