namespace ChugimCenter.Application.Common;

/// <summary>סוגי השגיאות העסקיות. שכבת ה-API מתרגמת כל סוג לסטטוס HTTP.</summary>
public enum ErrorType
{
    Validation = 1,    // 400
    Unauthorized = 2,  // 401
    Forbidden = 3,     // 403
    NotFound = 4,      // 404
    Conflict = 5       // 409
}
