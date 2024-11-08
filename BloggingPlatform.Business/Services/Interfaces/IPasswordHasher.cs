namespace BloggingPlatform.Business.Services.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    
    bool VerifyPassword(string passwordHash,string  inputPassword);
}