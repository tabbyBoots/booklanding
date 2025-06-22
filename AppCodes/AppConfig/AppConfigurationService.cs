using Microsoft.Extensions.Options;

namespace mvcDapper3.AppCodes.AppConfig;

/// <summary>
/// 型別安全設定存取服務實作
/// </summary>
public class AppConfigurationService : IAppConfigurationService
{
    private readonly IOptionsMonitor<DatabaseConfig> _databaseOptions;
    private readonly IOptionsMonitor<JwtConfig> _jwtOptions;
    private readonly IOptionsMonitor<SessionConfig> _sessionOptions;
    private readonly IOptionsMonitor<AppConfig> _appOptions;
    private readonly IConfigurationValidator _validator;
    private readonly ILogger<AppConfigurationService> _logger;

    public AppConfigurationService(
        IOptionsMonitor<DatabaseConfig> databaseOptions,
        IOptionsMonitor<JwtConfig> jwtOptions,
        IOptionsMonitor<SessionConfig> sessionOptions,
        IOptionsMonitor<AppConfig> appOptions,
        IConfigurationValidator validator,
        ILogger<AppConfigurationService> logger)
    {
        _databaseOptions = databaseOptions;
        _jwtOptions = jwtOptions;
        _sessionOptions = sessionOptions;
        _appOptions = appOptions;
        _validator = validator;
        _logger = logger;
    }

    public DatabaseConfig Database => _databaseOptions.CurrentValue;
    public JwtConfig Jwt => _jwtOptions.CurrentValue;
    public SessionConfig Session => _sessionOptions.CurrentValue;
    public AppConfig App => _appOptions.CurrentValue;

    public void ReloadConfigurations()
    {
        try
        {
            // IOptionsMonitor 會自動監控設定檔變更並重新載入
            _logger.LogInformation("設定已重新載入");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定重新載入時發生錯誤");
            throw;
        }
    }

    public IEnumerable<string> ValidateAllConfigurations()
    {
        var errors = new List<string>();

        try
        {
            // 驗證資料庫設定
            var databaseValidation = _validator.ValidateConfiguration(Database);
            if (databaseValidation != System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                errors.Add($"資料庫設定: {databaseValidation.ErrorMessage}");
            }

            // 驗證 JWT 設定
            var jwtValidation = _validator.ValidateConfiguration(Jwt);
            if (jwtValidation != System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                errors.Add($"JWT 設定: {jwtValidation.ErrorMessage}");
            }

            // 驗證 Session 設定
            var sessionValidation = _validator.ValidateConfiguration(Session);
            if (sessionValidation != System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                errors.Add($"Session 設定: {sessionValidation.ErrorMessage}");
            }

            // 驗證應用程式設定
            var appValidation = _validator.ValidateConfiguration(App);
            if (appValidation != System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                errors.Add($"應用程式設定: {appValidation.ErrorMessage}");
            }

            if (errors.Any())
            {
                _logger.LogError("設定驗證失敗: {Errors}", string.Join(", ", errors));
            }
            else
            {
                _logger.LogInformation("所有設定驗證通過");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定驗證時發生錯誤");
            errors.Add($"設定驗證時發生錯誤: {ex.Message}");
        }

        return errors;
    }
}
