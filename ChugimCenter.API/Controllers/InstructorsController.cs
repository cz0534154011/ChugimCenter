using ChugimCenter.API.Authorization;
using ChugimCenter.API.Extensions;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.Users;
using ChugimCenter.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChugimCenter.API.Controllers;

/// <summary>יצירת מדריכה – רק על ידי המנהלת.</summary>
[ApiController]
[Route("api/instructors")]
[Authorize(Roles = Roles.Admin)]
[ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
public class InstructorsController(IUserService userService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateInstructorDto dto, CancellationToken ct)
        => this.ToCreatedResult(await userService.CreateInstructorAsync(dto, ct),
            nameof(UsersController.GetById), "Users", user => new { id = user.Id });
}
