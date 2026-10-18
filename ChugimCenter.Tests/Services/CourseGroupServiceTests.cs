using ChugimCenter.Application.Common;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Application.Services;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Domain.Enums;
using ChugimCenter.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ChugimCenter.Tests.Services;

/// <summary>
/// הרשאות לרשימת התלמידות של קבוצה: רק המדריכה של הקבוצה או המנהלת.
/// ההרשאה נבדקת ב-Service מול הנתונים ("האם זו הקבוצה שלך"), ולא רק לפי התפקיד שב-token.
/// </summary>
public class CourseGroupServiceTests
{
    private readonly Mock<ICourseGroupRepository> _groups = new();
    private readonly Mock<IEnrollmentRepository> _enrollments = new();
    private readonly CourseGroup _group = TestData.Group(enrolledCount: 1);
    private readonly CourseGroupService _service;

    public CourseGroupServiceTests()
    {
        _service = new CourseGroupService(_groups.Object, Mock.Of<ICategoryRepository>(), Mock.Of<IUserRepository>(),
            _enrollments.Object, TestMapper.Instance, TestData.Clock(), NullLogger<CourseGroupService>.Instance);

        // בקבוצה יש תלמידה אחת, חנה, עם רישום פעיל
        var student = TestData.ActiveEnrollment(TestData.Member(), _group);
        _groups.Setup(r => r.GetByIdAsync(_group.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_group);
        _enrollments.Setup(r => r.GetActiveByGroupAsync(_group.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Enrollment> { student });
    }

    [Fact]
    public async Task GetStudents_WhenOtherInstructor_ReturnsForbidden()
    {
        // Arrange: מדריכה מחוברת, אבל לא המדריכה של הקבוצה הזו
        var otherInstructor = new CurrentUser(TestData.OtherInstructorId, UserRole.Instructor);

        // Act
        var result = await _service.GetStudentsAsync(_group.Id, otherInstructor, CancellationToken.None);

        // Assert: 403, ופרטי התלמידות לא נשלפו בכלל
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
        Assert.Equal("אינך המדריכה של קבוצה זו", result.Error);
        _enrollments.Verify(r => r.GetActiveByGroupAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(TestData.InstructorId, UserRole.Instructor)] // המדריכה של הקבוצה
    [InlineData(TestData.AdminId, UserRole.Admin)]           // המנהלת, שרשאית לכל הקבוצות
    public async Task GetStudents_WhenGroupInstructorOrAdmin_ReturnsStudents(int userId, UserRole role)
    {
        // Act
        var result = await _service.GetStudentsAsync(_group.Id, new CurrentUser(userId, role), CancellationToken.None);

        // Assert: הרשימה חוזרת, עם פרטי התלמידה אחרי ההמרה ל-DTO
        Assert.True(result.IsSuccess);
        var student = Assert.Single(result.Value!);
        Assert.Equal(TestData.MemberId, student.UserId);
        Assert.Equal("חנה גולד", student.FullName);
    }
}
