using ChugimCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChugimCenter.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.HasKey(e => e.Id);

        // many-to-many בין משתתפות לקבוצות, דרך טבלת הרישומים
        builder.HasOne(e => e.User)
               .WithMany(u => u.Enrollments)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CourseGroup)
               .WithMany(g => g.Enrollments)
               .HasForeignKey(e => e.CourseGroupId)
               .OnDelete(DeleteBehavior.Restrict);

        // מי ביטלה את הרישום (המשתתפת עצמה או מנהלת)
        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(e => e.CancelledById)
               .OnDelete(DeleteBehavior.Restrict);

        // רשת ביטחון: רישום פעיל אחד בלבד לכל משתתפת בכל קבוצה (רישומים מבוטלים לא נספרים)
        builder.HasIndex(e => new { e.UserId, e.CourseGroupId })
               .IsUnique()
               .HasFilter("\"Status\" = 1");

        builder.HasData(SeedData.Enrollments);
    }
}
