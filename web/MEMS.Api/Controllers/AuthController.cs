using MEMS.Api.Auth;
using MEMS.Data.Entities;
using MEMS.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Controllers;

// Token login for API clients (Swagger, future apps). The Admin site signs in with a cookie instead.
[ApiController]
[Route("api/auth")]
public class AuthController(UserManager<ApplicationUser> userManager, JwtTokenService tokens, ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.UserName);
        if (user is null)
        {
            logger.LogWarning("Failed API login for unknown user {UserName}", request.UserName);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid user name or password.");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "This account is temporarily locked. Try again later.");
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            logger.LogWarning("Failed API login for {UserName}", user.UserName);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid user name or password.");
        }

        await userManager.ResetAccessFailedCountAsync(user);
        var roles = await userManager.GetRolesAsync(user);
        logger.LogInformation("API login succeeded for {UserName}", user.UserName);
        return Ok(tokens.CreateToken(user, roles));
    }
}
