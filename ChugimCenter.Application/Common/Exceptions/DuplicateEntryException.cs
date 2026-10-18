namespace ChugimCenter.Application.Common.Exceptions;

/// <summary>
/// ניסיון לשמור ערך שכבר קיים בעמודה ייחודית (למשל אימייל, או רישום פעיל כפול).
/// שכבת Infrastructure ממירה אליה את שגיאת האינדקס הייחודי של בסיס הנתונים.
/// </summary>
public class DuplicateEntryException(Exception? inner = null)
    : Exception("A record with the same unique value already exists.", inner);
