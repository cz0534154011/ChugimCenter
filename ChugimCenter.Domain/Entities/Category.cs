namespace ChugimCenter.Domain.Entities;

/// <summary>תחום פעילות, למשל ציור או דרמה.</summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // קשר one-to-many: לתחום יש הרבה קבוצות
    public ICollection<CourseGroup> CourseGroups { get; set; } = new List<CourseGroup>();
}
