namespace ChugimCenter.Application.Common.Exceptions;

/// <summary>
/// השורה השתנתה על ידי בקשה אחרת בין הקריאה לשמירה (התנגשות גרסה).
/// שכבת Infrastructure ממירה אליה את החריגה של EF, כדי ש-Application לא יכיר את EF.
/// </summary>
public class ConcurrencyConflictException(Exception? inner = null)
    : Exception("The data was changed by another request.", inner);
