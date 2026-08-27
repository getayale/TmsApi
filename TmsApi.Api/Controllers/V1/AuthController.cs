using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Identity;

namespace TmsApi.Api.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<TmsUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(
        UserManager<TmsUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }


    // =========================
    // REGISTER
    // =========================

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
        // Check whether the email already exists
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            // Generic response prevents account enumeration
            return Ok(new
            {
                message = "Registration request received."
            });
        }


        // Create Identity user
        var user = new TmsUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };


        // UserManager handles password hashing
        var result =
            await _userManager.CreateAsync(
                user,
                request.Password);


        if (!result.Succeeded)
        {
            var errors =
                result.Errors
                    .Select(e => e.Description);

            return BadRequest(new
            {
                errors
            });
        }


        // Make sure requested role exists
        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(request.Role));
        }


        // Assign role to user
        await _userManager.AddToRoleAsync(
            user,
            request.Role);


        return Ok(new
        {
            message = "Registration successful."
        });
    }


    // =========================
    // LOGIN
    // =========================

    public record LoginRequest(
        string Email,
        string Password);


    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user =
            await _userManager.FindByEmailAsync(
                request.Email);


        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }


        // Check whether account is locked
        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(423, new
            {
                detail =
                    "Account locked due to multiple failed login attempts. " +
                    "Try again in 15 minutes."
            });
        }


        // Verify password
        var validPassword =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);


        if (!validPassword)
        {
            // Increment failed login counter
            await _userManager.AccessFailedAsync(user);

            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }


        // Successful login → reset failed attempts
        await _userManager.ResetAccessFailedCountAsync(user);


        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName
        });
    }
}