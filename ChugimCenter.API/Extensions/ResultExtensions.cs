using ChugimCenter.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ChugimCenter.API.Extensions;

/// <summary>
/// התרגום היחיד במערכת מ-Result של ה-Services לתשובת HTTP.
/// ה-Controllers קוראים לו, ולכן אף Controller לא מחליט בעצמו על סטטוס של שגיאה.
/// </summary>
public static class ResultExtensions
{
    /// <summary>פעולה בלי נתונים: הצלחה היא 204.</summary>
    public static IActionResult ToActionResult(this ControllerBase controller, Result result)
        => result.IsSuccess ? controller.NoContent() : controller.ToErrorResult(result);

    /// <summary>פעולה עם נתונים: הצלחה היא 200 עם הנתונים.</summary>
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
        => result.IsSuccess ? controller.Ok(result.Value) : controller.ToErrorResult(result);

    /// <summary>יצירה: הצלחה היא 201, עם כותרת Location שמצביעה על המשאב החדש.</summary>
    public static IActionResult ToCreatedResult<T>(
        this ControllerBase controller, Result<T> result,
        string actionName, string? controllerName, Func<T, object> routeValues)
        => result.IsSuccess
            ? controller.CreatedAtAction(actionName, controllerName, routeValues(result.Value!), result.Value)
            : controller.ToErrorResult(result);

    private static ObjectResult ToErrorResult(this ControllerBase controller, Result result)
    {
        var status = result.ErrorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return controller.StatusCode(status,
            ErrorResponseFactory.Create(controller.HttpContext, status, result.Error ?? "אירעה שגיאה"));
    }
}
