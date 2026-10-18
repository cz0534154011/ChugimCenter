using ChugimCenter.API.Extensions;
using ChugimCenter.Application.DTOs.Auth;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChugimCenter.API.Controllers;

/// <summary>הרשמה והתחברות. פתוח לכולם. הסיסמה עוברת רק בגוף הבקשה.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>הרשמה עצמית של משתתפת. מחזירה token, כדי שאפשר יהיה להמשיך מחוברת.</summary>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        => this.ToCreatedResult(await authService.RegisterAsync(dto, ct),
            nameof(UsersController.GetMe), "Users", _ => new { });

    [HttpPost("login")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        => this.ToActionResult(await authService.LoginAsync(dto, ct));
}
