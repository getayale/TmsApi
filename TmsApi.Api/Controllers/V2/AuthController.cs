using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Auth;

namespace TmsApi.Api.Controllers.v2;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("2.0")]
[ApiExplorerSettings(GroupName = "v2")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login(
        [FromBody] LoginRequest request,
        [FromServices] IWebHostEnvironment env)
    {
        // Demo admin credentials for Module 10
        if (request.Username != "admin" ||
            request.Password != "Password123!")
        {
            return Unauthorized(new
            {
                detail = "Invalid username or password."
            });
        }

        var dummyJwt = "header.payload.signature-demo-token";

        Response.Cookies.Append(
            "tms_auth",
            dummyJwt,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = !env.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(2)
            });

        return Ok(
            new UserProfileDto(
                "System Admin",
                "Admin"
            )
        );
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        // Browser automatically sends the HttpOnly cookie
        if (!Request.Cookies.ContainsKey("tms_auth"))
        {
            return Unauthorized(new
            {
                detail = "Session expired or missing authentication cookie."
            });
        }

        return Ok(
            new UserProfileDto(
                "System Admin",
                "Admin"
            )
        );
    }
}