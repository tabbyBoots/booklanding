
namespace mvcDapper3.AppCodes.AppInterface
{
    /// <summary>
    /// 加解密服務介面
    /// </summary>
    public interface ICrypto
    {
        /// <summary>
        /// 建立密碼雜湊
        /// </summary>
        string CreatePasswordHash(string password);

        /// <summary>
        /// 驗證密碼是否符合儲存的雜湊值
        /// </summary>
        bool VerifyPassword(string password, string storedHash);

        /// <summary>
        /// PBKDF2迭代次數配置
        /// </summary>
        int Iterations { get; set; }
    }
}
