using System.Security.Cryptography;
using BloggingPlatform.Business.Services.Interfaces;

namespace BloggingPlatform.Business.Services.Providers;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 128 / 8;
    private const int KeySize = 256 / 8;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName HashAlgorithmName= HashAlgorithmName.SHA256;
    private const char Delimiter = ';';
    
    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName, KeySize);

        return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string passwordHash, string inputPassword)
    {
        var elements = passwordHash.Split(Delimiter);
        var salt = Convert.FromBase64String(elements[0]);
        var hash = Convert.FromBase64String(elements[1]);
        
        var hashInput = Rfc2898DeriveBytes.Pbkdf2(inputPassword, salt, Iterations, HashAlgorithmName, KeySize);

        return CryptographicOperations.FixedTimeEquals(hash, hashInput);
    }
}