using ChugimCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChugimCenter.Infrastructure.Persistence.Configurations;

public class CourseGroupConfiguration : IEntityTypeConfiguration<CourseGroup>
{
    public void Configure(EntityTypeBuilder<CourseGroup> builder)
    {
        // הגנה נוספת ברמת בסיס הנתונים: מספר הנרשמות לעולם לא יחרוג ממספר המקומות
        builder.ToTable("CourseGroups", t => t.HasCheckConstraint(
            "CK_CourseGroups_EnrolledCount",
            "\"EnrolledCount\" >= 0 AND \"EnrolledCount\" <= \"Capacity\""));

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
        builder.Property(g => g.Price).HasPrecision(10, 2);

        // ה-concurrency token: EF מוסיף את הערך הישן לתנאי ה-WHERE בכל עדכון
        builder.Property(g => g.Version).IsConcurrencyToken();

        // one-to-many: תחום ← קבוצות
        builder.HasOne(g => g.Category)
               .WithMany(c => c.CourseGroups)
               .HasForeignKey(g => g.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        // one-to-many: מדריכה ← קבוצות
        builder.HasOne(g => g.Instructor)
               .WithMany(u => u.InstructedGroups)
               .HasForeignKey(g => g.InstructorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedData.CourseGroups);
    }
}
