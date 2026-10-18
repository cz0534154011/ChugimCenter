using ChugimCenter.API.Authorization;
using ChugimCenter.API.Extensions;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.CourseGroups;
using ChugimCenter.Application.DTOs.Enrollments;
using ChugimCenter.Application.DTOs.Users;
using ChugimCenter.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChugimCenter.API.Controllers;

/// <summary>המשתמשת המחוברת (me), והרשימות למנהלת.</summary>
[ApiController]
[Route("api/users")]
[Authorize]
[ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
public class UsersController(
    IUserService userService,
    IEnrollmentService enrollmentService,
    ICourseGroupService courseGroupService) : ControllerBase
{
    /// <summary>הפרטים שלי.</summary>
    [HttpGet("me")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
        => this.ToActionResult(await userService.GetByIdAsync(User.ToCurrentUser().Id, ct));

    /// <summary>"החוגים שלי". כברירת מחדל רק רישומים פעילים; Status=Cancelled מציג מבוטלים.</summary>
    [HttpGet("me/enrollments")]
    [Authorize(Roles = Roles.Member)]
    [ProducesResponseType<PagedResult<EnrollmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyEnrollments([FromQuery] EnrollmentQueryDto query, CancellationToken ct)
        => Ok(await enrollmentService.GetMineAsync(query, User.ToCurrentUser().Id, ct));

    /// <summary>"הקבוצות שלי" של המדריכה המחוברת.</summary>
    [HttpGet("me/course-groups")]
    [Authorize(Roles = Roles.Instructor)]
    [ProducesResponseType<List<CourseGroupDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyCourseGroups(CancellationToken ct)
        => Ok(await courseGroupService.GetInstructorGroupsAsync(User.ToCurrentUser().Id, ct));

    /// <summary>רשימת המשתמשות, עם סינון לפי תפקיד (למנהלת).</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<PagedResult<UserDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] UserQueryDto query, CancellationToken ct)
        => Ok(await userService.GetPagedAsync(query, ct));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        => this.ToActionResult(await userService.GetByIdAsync(id, ct));
}
