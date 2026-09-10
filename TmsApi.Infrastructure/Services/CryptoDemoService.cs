namespace TmsApi.Infrastructure.Services;
public class CryptoDemoService
{
    public string HashUserPassword(string plaintext)
    {
        
        return BCrypt.Net.BCrypt.HashPassword(plaintext,workFactor:12);
    }
    public bool VerifyUserPassword(string plaintext,string hashedDbPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plaintext,hashedDbPassword);
        
    }
}