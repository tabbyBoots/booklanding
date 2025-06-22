using System.Security;
using System.Security.Cryptography;
using System.Text;


/// <summary>
/// 加解密功能服務
/// </summary>
public class CryptographyService : ICrypto
{
    public const string HashPrefix = "$v1$"; // Version-only identifier
    public int Iterations { get; set; } = 210000; // OWASP 2025 recommended minimum


    /// <summary>
    /// 建立PBKDF2密碼雜湊
    /// </summary>
    public string CreatePasswordHash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            64
        );
        return $"{HashPrefix}{Convert.ToBase64String(salt)}${Iterations}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// 驗證密碼
    /// </summary>
    public bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length != 4 || parts[0] != "v1")
        {
            throw new SecurityException("Invalid hash format");
        }

        // Validate iteration count meets current security standards
        const int minIterations = 210000;
        if (!int.TryParse(parts[2], out int iterations) || iterations < minIterations)
        {
            throw new SecurityException($"Iteration count must be at least {minIterations}");
        }

        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] hash = Convert.FromBase64String(parts[3]);

        // Validate salt size
        if (salt.Length < 16)
        {
            throw new SecurityException("Invalid salt length");
        }

        // Validate hash size
        if (hash.Length != 64)
        {
            throw new SecurityException("Invalid hash length");
        }
        
        byte[] testHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA512,
            64
        );
        return CryptographicOperations.FixedTimeEquals(hash, testHash);
    }
}

public interface ICrypto
{
    string CreatePasswordHash(string password);
    bool VerifyPassword(string password, string storedHash);
    int Iterations { get; set; }
}
