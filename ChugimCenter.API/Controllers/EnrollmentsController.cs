using ChugimCenter.API.Authorization;
using ChugimCenter.API.Extensions;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.Enrollments;
using ChugimCenter.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChugimCenter.API.Controllers;

/// <summary>
/// רישומים לחוגים – המשאב המוגבל.
/// התחרות על המקום האחרון מטופלת ב-EnrollmentService, ומוחזרת כאן כ-409.
/// </summary>
[ApiController]
[Route("api/enrollments")]
[Authorize]
[ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    /// <summary>רישום לחוג. המשתתפת נלקחת מה-token, לא מהבקשה.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Member)]
    [ProducesResponseType<EnrollmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll([FromBody] CreateEnrollmentDto dto, CancellationToken ct)
        => this.ToCreatedResult(await enrollmentService.EnrollAsync(dto, User.ToCurrentUser(), ct),
            nameof(GetById), null, enrollment => new { id = enrollment.Id });

    /// <summary>רישום בודד – לבעלת הרישום או למנהלת.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<EnrollmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        => this.ToActionResult(await enrollmentService.GetByIdAsync(id, User.ToCurrentUser(), ct));

    /// <summary>ביטול רישום. השורה לא נמחקת מבסיס הנתונים – הסטטוס משתנה ל"מבוטל".</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel([FromRoute] int id, CancellationToken ct)
        => this.ToActionResult(await enrollmentService.CancelAsync(id, User.ToCurrentUser(), ct));

    /// <summary>כל הרישומים, עם סינון לפי קבוצה, סטטוס ותשלום (למנהלת).</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<PagedResult<EnrollmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] EnrollmentQueryDto query, CancellationToken ct)
        => Ok(await enrollmentService.GetPagedAsync(query, ct));

    /// <summary>סימון תשלום (למנהלת): {"isPaid": true}</summary>
    [HttpPut("{id:int}/payment")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<EnrollmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetPayment([FromRoute] int id, [FromBody] SetPaymentDto dto, CancellationToken ct)
        => this.ToActionResult(await enrollmentService.SetPaymentAsync(id, dto.IsPaid, User.ToCurrentUser().Id, ct));
}
