using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/crypto-demo")]
public class CryptoDemoController : ControllerBase
{
    [HttpGet("test")]
    public IActionResult Test()
    {
        var service = new CryptoDemoService();

        string password = "Password123!";

        // Hash the same password twice
        string hash1 = service.HashUserPassword(password);
        string hash2 = service.HashUserPassword(password);

        // Verify both hashes
        bool match1 = service.VerifyUserPassword(password, hash1);
        bool match2 = service.VerifyUserPassword(password, hash2);

        // Test an incorrect password too
        bool wrongPassword = service.VerifyUserPassword(
            "WrongPassword!",
            hash1);

        return Ok(new
        {
            Password = password,
            Hash1 = hash1,
            Hash2 = hash2,
            HashesAreDifferent = hash1 != hash2,
            Match1 = match1,
            Match2 = match2,
            WrongPassword = wrongPassword
        });
    }
}