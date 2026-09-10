using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<TmsUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TmsDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<TmsUser> userManager,
        RoleManager<IdentityRole> roleManager,
        TmsDbContext context,
        TokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _tokenService = tokenService;
    }

    public record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string Role);

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return Ok(new
            {
                message = "Registration request received."
            });
        }

        var user = new TmsUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => e.Description);

            return BadRequest(new { errors });
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(request.Role));
        }

        await _userManager.AddToRoleAsync(
            user,
            request.Role);

        return Ok(new
        {
            message = "Registration successful."
        });
    }

    public record LoginRequest(
        string Email,
        string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user =
            await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(423, new
            {
                detail =
                    "Account locked due to multiple failed login attempts."
            });
        }

        var validPassword =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);

            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        // Get the user's roles.
        var roles = await _userManager.GetRolesAsync(user);

        // Generate the short-lived access token.
        var accessToken =
            _tokenService.GenerateJwt(user, roles);

        // Create the initial refresh token.
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken,
            refreshToken = refreshToken.Token
        });
    }

    public record RefreshRequest(string RefreshToken);

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request)
    {
        var storedToken =
            await _context.RefreshTokens
                .FirstOrDefaultAsync(
                    rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid refresh token."
            });
        }

        // If an already-used token is submitted,
        // treat it as possible token theft.
        if (storedToken.IsUsed)
        {
            var userTokens =
                await _context.RefreshTokens
                    .Where(rt => rt.UserId == storedToken.UserId)
                    .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            return Unauthorized(new
            {
                detail =
                    "Token theft detected. All user sessions revoked."
            });
        }

        // Check whether the refresh token is revoked or expired.
        if (storedToken.IsRevoked ||
            storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized(new
            {
                detail = "Refresh token expired or revoked."
            });
        }

        // Invalidate the current refresh token.
        storedToken.IsUsed = true;

        // Create a completely new refresh token.
        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);

        await _context.SaveChangesAsync();

        // Find the user who owns the refresh token.
        var user =
            await _userManager.FindByIdAsync(
                storedToken.UserId);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "User no longer exists."
            });
        }

        // Get the user's current roles.
        var roles =
            await _userManager.GetRolesAsync(user);

        // Generate a new 15-minute access token.
        var newAccessToken =
            _tokenService.GenerateJwt(user, roles);

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token
        });
    }
}