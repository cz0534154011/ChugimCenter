using ChugimCenter.API.Authorization;
using ChugimCenter.API.Extensions;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.CourseGroups;
using ChugimCenter.Application.DTOs.Users;
using ChugimCenter.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChugimCenter.API.Controllers;

/// <summary>קבוצות חוג. קריאה פתוחה לכולם; שינויים רק למנהלת; רשימת תלמידות למדריכה של הקבוצה ולמנהלת.</summary>
[ApiController]
[Route("api/course-groups")]
public class CourseGroupsController(ICourseGroupService courseGroupService) : ControllerBase
{
    /// <summary>רשימה עם סינון, מיון ועמודים. כברירת מחדל רק חוגים שטרם הסתיימו.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<CourseGroupDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] CourseGroupQueryDto query, CancellationToken ct)
        => Ok(await courseGroupService.GetPagedAsync(query, User.ToCurrentUserOrNull(), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType<CourseGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        => this.ToActionResult(await courseGroupService.GetByIdAsync(id, User.ToCurrentUserOrNull(), ct));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<CourseGroupDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CourseGroupRequestDto dto, CancellationToken ct)
        => this.ToCreatedResult(await courseGroupService.CreateAsync(dto, ct),
            nameof(GetById), null, group => new { id = group.Id });

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<CourseGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CourseGroupRequestDto dto, CancellationToken ct)
        => this.ToActionResult(await courseGroupService.UpdateAsync(id, dto, ct));

    /// <summary>הפעלה או השבתה: {"isActive": false}</summary>
    [HttpPatch("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetActive([FromRoute] int id, [FromBody] SetActiveDto dto, CancellationToken ct)
        => this.ToActionResult(await courseGroupService.SetActiveAsync(id, dto.IsActive, ct));

    /// <summary>מחיקה – רק כשלא היו בקבוצה רישומים בכלל. אחרת 409 עם הצעה להשבית.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        => this.ToActionResult(await courseGroupService.DeleteAsync(id, ct));

    /// <summary>רשימת התלמידות בקבוצה. מדריכה אחרת מקבלת 403 – נבדק ב-Service מול בסיס הנתונים.</summary>
    [HttpGet("{id:int}/students")]
    [Authorize(Roles = Roles.AdminOrInstructor)]
    [ProducesResponseType<List<StudentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudents([FromRoute] int id, CancellationToken ct)
        => this.ToActionResult(await courseGroupService.GetStudentsAsync(id, User.ToCurrentUser(), ct));
}
