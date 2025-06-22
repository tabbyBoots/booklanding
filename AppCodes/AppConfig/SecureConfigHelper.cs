using System.Security.Cryptography;
using System.Text;

namespace mvcDapper3.AppCodes.AppConfig;

/// <summary>
/// 敏感資訊保護幫助類別
/// </summary>
public static class SecureConfigHelper
{
    /// <summary>
    /// 從環境變數取得敏感設定，如果沒有則使用預設值
    /// </summary>
    /// <param name="environmentKey">環境變數名稱</param>
    /// <param name="defaultValue">預設值</param>
    /// <param name="isRequired">是否必要</param>
    /// <returns>設定值</returns>
    public static string GetSecureValue(string environmentKey, string defaultValue = "", bool isRequired = false)
    {
        var value = Environment.GetEnvironmentVariable(environmentKey);
        
        if (string.IsNullOrEmpty(value))
        {
            if (isRequired)
            {
                throw new InvalidOperationException($"必要的環境變數 '{environmentKey}' 未設定");
            }
            return defaultValue;
        }
        
        return value;
    }
    
    /// <summary>
    /// 生成安全的隨機金鑰
    /// </summary>
    /// <param name="length">金鑰長度</param>
    /// <returns>Base64 編碼的金鑰</returns>
    public static string GenerateSecureKey(int length = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
    
    /// <summary>
    /// 遮蔽敏感資訊用於日誌記錄
    /// </summary>
    /// <param name="sensitive">敏感字串</param>
    /// <param name="visibleChars">可見字元數量</param>
    /// <returns>遮蔽後的字串</returns>
    public static string MaskSensitiveData(string sensitive, int visibleChars = 4)
    {
        if (string.IsNullOrEmpty(sensitive))
            return string.Empty;
            
        if (sensitive.Length <= visibleChars * 2)
            return new string('*', sensitive.Length);
            
        return $"{sensitive[..visibleChars]}***{sensitive[^visibleChars..]}";
    }
}
