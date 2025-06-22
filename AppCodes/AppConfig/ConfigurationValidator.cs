using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace mvcDapper3.AppCodes.AppConfig;

/// <summary>
/// 設定驗證服務介面
/// </summary>
public interface IConfigurationValidator
{
    /// <summary>
    /// 驗證設定物件
    /// </summary>
    /// <typeparam name="T">設定類型</typeparam>
    /// <param name="config">設定物件</param>
    /// <returns>驗證結果</returns>
    ValidationResult ValidateConfiguration<T>(T config) where T : class;
}

/// <summary>
/// 設定驗證服務實作
/// </summary>
public class ConfigurationValidator : IConfigurationValidator
{
    public ValidationResult ValidateConfiguration<T>(T config) where T : class
    {
        var context = new ValidationContext(config);
        var results = new List<ValidationResult>();
        
        bool isValid = Validator.TryValidateObject(config, context, results, validateAllProperties: true);
        
        if (!isValid)
        {
            var errorMessages = results.Select(r => r.ErrorMessage).ToList();
            return new ValidationResult($"設定驗證失敗: {string.Join(", ", errorMessages)}");
        }
        
        return ValidationResult.Success!;
    }
}
