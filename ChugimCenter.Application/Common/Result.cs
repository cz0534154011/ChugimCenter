namespace ChugimCenter.Application.Common;

/// <summary>תוצאה של פעולה שאינה מחזירה נתונים: הצלחה, או כישלון עם סוג שגיאה והודעה.</summary>
public class Result
{
    public bool IsSuccess { get; }
    public ErrorType? ErrorType { get; }
    public string? Error { get; }

    protected Result(bool isSuccess, ErrorType? errorType, string? error)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        Error = error;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(ErrorType type, string error) => new(false, type, error);

    /// <summary>
    /// מעביר כישלון הלאה, כתוצאה מסוג אחר (למשל מ-Result&lt;User&gt; ל-Result&lt;UserDto&gt;).
    /// נקרא רק על תוצאה שנכשלה.
    /// </summary>
    public Result<T> ToFailure<T>() => Result<T>.Failure(ErrorType!.Value, Error!);
}

/// <summary>תוצאה של פעולה שמחזירה נתונים מסוג T.</summary>
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(true, null, null) => Value = value;
    private Result(ErrorType type, string error) : base(false, type, error) { }

    public static Result<T> Success(T value) => new(value);
    public static new Result<T> Failure(ErrorType type, string error) => new(type, error);
}
