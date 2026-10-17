namespace ChugimCenter.Domain.Entities;

/// <summary>קבוצת חוג. זה המשאב המוגבל שמתחרים עליו.</summary>
public class CourseGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int InstructorId { get; set; }
    public User Instructor { get; set; } = null!;

    public DateOnly StartDate { get; set; }
    public int SessionsCount { get; set; }

    // מתמלא אוטומטית מתאריך ההתחלה בזמן השמירה (בשכבת Infrastructure)
    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }
    public int DurationMinutes { get; set; }

    public int MinAge { get; set; }
    public int MaxAge { get; set; }
    public decimal Price { get; set; }

    public int Capacity { get; set; }

    // מתעדכן ברישום ובביטול, כדי שכל תחרות על מקום "תיפגש" בשורה הזו
    public int EnrolledCount { get; set; }

    public bool IsActive { get; set; } = true;

    // ה-concurrency token. מוגדר כ-token ב-Fluent API, ומתחלף אוטומטית בכל שמירה
    public Guid Version { get; set; } = Guid.NewGuid();

    // קשר many-to-many עם המשתתפות, דרך הרישומים
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
