using System.Security.Claims;
using MEMS.Api.Auth;
using MEMS.Data.Entities;
using MEMS.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MEMS.Api.Controllers;

// Registration and login. The mobile app registers residents (no role, no tenant); administrators are created by a Super Admin.
[ApiController]
[Route("api/auth")]
public class AuthController(UserManager<ApplicationUser> users, JwtTokenService tokens, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim();
        var user = new ApplicationUser { UserName = email, Email = email, Name = request.Name.Trim() };

        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            // The user name is the email, so a taken email would otherwise be reported twice.
            var emailTaken = result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateEmail));
            foreach (var error in result.Errors)
            {
                if (emailTaken && error.Code == nameof(IdentityErrorDescriber.DuplicateUserName))
                {
                    continue;
                }
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        logger.LogInformation("New account registered: {UserId}", user.Id);
        return Ok(new { message = "Account created. You can now log in." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var login = request.Email.Trim();
        var user = await users.FindByNameAsync(login) ?? await users.FindByEmailAsync(login);
        if (user is null || await users.IsLockedOutAsync(user) || !await users.CheckPasswordAsync(user, request.Password))
        {
            if (user is not null)
            {
                await users.AccessFailedAsync(user);
            }
            logger.LogWarning("Failed login attempt for {Login}", login);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.");
        }

        await users.ResetAccessFailedCountAsync(user);
        return Ok(new TokenResponse(await tokens.CreateTokenAsync(user)));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var user = await users.FindByIdAsync(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? string.Empty);
        if (user is null)
        {
            return Unauthorized();
        }
        return new UserResponse(user.Name, user.Email ?? string.Empty, [.. await users.GetRolesAsync(user)], user.MunicipalityId);
    }
}
