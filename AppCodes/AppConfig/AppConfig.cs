using System.ComponentModel.DataAnnotations;

namespace mvcDapper3.AppCodes.AppConfig;

/// <summary>
/// 應用程式通用設定類別 - 強型別設定
/// </summary>
public class AppConfig
{
    public const string SectionName = "AppSettings";
    
    /// <summary>
    /// 應用程式名稱
    /// </summary>
    [Required(ErrorMessage = "應用程式名稱不能為空")]
    public string AppName { get; set; } = "MvcDapper3";
    
    /// <summary>
    /// 應用程式版本
    /// </summary>
    public string Version { get; set; } = "1.0.0";
    
    /// <summary>
    /// 是否啟用詳細錯誤訊息
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;
    
    /// <summary>
    /// 是否啟用 Swagger
    /// </summary>
    public bool EnableSwagger { get; set; } = true;
    
    /// <summary>
    /// 檔案上傳大小限制 (MB)
    /// </summary>
    [Range(1, 1024, ErrorMessage = "檔案上傳大小限制必須在 1 到 1024 MB 之間")]
    public int MaxFileUploadSizeMB { get; set; } = 10;
    
    /// <summary>
    /// 分頁大小預設值
    /// </summary>
    [Range(5, 100, ErrorMessage = "分頁大小必須在 5 到 100 之間")]
    public int DefaultPageSize { get; set; } = 10;
    
    /// <summary>
    /// 允許的檔案類型
    /// </summary>
    public string[] AllowedFileTypes { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };
}
