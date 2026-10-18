using System.Globalization;
using ChugimCenter.Application.Common;
using ChugimCenter.Application.Common.Exceptions;
using ChugimCenter.Application.DTOs.Enrollments;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Application.Services;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Domain.Enums;
using ChugimCenter.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChugimCenter.Tests.Services;

/// <summary>
/// בדיקות הלוגיקה של המשאב המוגבל, מקום בקבוצה (סעיף 13 בהוראות).
/// ה-Repositories מדומים ב-Moq, ולכן נבדקת רק הלוגיקה של ה-Service, בלי בסיס נתונים.
/// xUnit יוצר מופע חדש של המחלקה לכל בדיקה, ולכן כל בדיקה מתחילה עם Mocks נקיים.
/// </summary>
public class EnrollmentServiceTests
{
    private static readonly CurrentUser MemberUser = new(TestData.MemberId, UserRole.Member);

    private readonly Mock<IEnrollmentRepository> _enrollments = new();
    private readonly Mock<ICourseGroupRepository> _groups = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ILogger<EnrollmentService>> _logger = new();
    private readonly EnrollmentService _service;

    public EnrollmentServiceTests()
    {
        _service = new EnrollmentService(_enrollments.Object, _groups.Object, _categories.Object,
            _users.Object, TestMapper.Instance, TestData.Clock(), _logger.Object);
    }

    [Fact]
    public async Task Enroll_WhenSeatAvailable_SavesEnrollmentAndIncrementsCount()
    {
        // Arrange: קבוצה עם 2 מקומות ונרשמת אחת, כלומר נשאר מקום אחרון
        var group = TestData.Group(capacity: 2, enrolledCount: 1);
        Arrange(group);

        // Act
        var result = await EnrollAsync(group);

        // Assert: הרישום נוצר, מספר הנרשמות עלה, והכל נשמר בשמירה אחת
        Assert.True(result.IsSuccess);
        Assert.Equal(EnrollmentStatus.Active, result.Value!.Status);
        Assert.Equal(group.Id, result.Value.CourseGroupId);
        Assert.Equal(2, group.EnrolledCount);
        _enrollments.Verify(r => r.AddAsync(
            It.Is<Enrollment>(e => e.UserId == TestData.MemberId && e.CourseGroupId == group.Id),
            It.IsAny<CancellationToken>()), Times.Once);
        _enrollments.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Enroll_WhenGroupIsFull_ReturnsConflictAndSavesNothing()
    {
        // Arrange: כל המקומות תפוסים
        var group = TestData.Group(capacity: 2, enrolledCount: 2);
        Arrange(group);

        // Act
        var result = await EnrollAsync(group);

        // Assert: נדחה עם 409, ושום דבר לא נוסף ולא נשמר
        AssertFailed(result, ErrorType.Conflict);
        Assert.Equal("אין מקומות פנויים בקבוצה", result.Error);
        Assert.Equal(2, group.EnrolledCount);
        _enrollments.Verify(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Enroll_WhenSeatTakenConcurrently_ReturnsConflictAndLogsWarning()
    {
        // Arrange: כל הבדיקות עוברות, אבל בזמן השמירה מתברר שבקשה אחרת כבר עדכנה את הקבוצה
        var group = TestData.Group(capacity: 2, enrolledCount: 1);
        Arrange(group);
        _enrollments.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ConcurrencyConflictException());

        // Act
        var result = await EnrollAsync(group);

        // Assert: 409 עם הודעה ברורה, ושורת לוג אחת ברמת Warning
        AssertFailed(result, ErrorType.Conflict);
        Assert.Equal("המקום נתפס על ידי משתמשת אחרת. נסי שוב.", result.Error);
        _logger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Theory]
    [InlineData("2019-01-01")] // בת 7 בתחילת החוג: צעירה מדי
    [InlineData("2015-01-01")] // בת 11 בתחילת החוג: מבוגרת מדי
    public async Task Enroll_WhenAgeOutOfRange_ReturnsValidation(string birthDate)
    {
        // Arrange
        var group = TestData.Group();
        Arrange(group, TestData.Member(DateOnly.Parse(birthDate, CultureInfo.InvariantCulture)));

        // Act
        var result = await EnrollAsync(group);

        // Assert
        AssertFailed(result, ErrorType.Validation);
        Assert.Equal("קבוצה זו מיועדת לגילאי 8–10", result.Error);
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Enroll_WhenOverlapsAnotherEnrollment_ReturnsConflictWithGroupName()
    {
        // Arrange: המשתתפת כבר רשומה לחוג אחר באותו יום ראשון, 16:30–17:30, שחופף ל-16:00–17:00
        var group = TestData.Group();
        var other = TestData.Group();
        other.Id = 5;
        other.Name = "התעמלות קרקע";
        other.StartTime = new TimeOnly(16, 30);
        Arrange(group, sameDayGroups: [other]);

        // Act
        var result = await EnrollAsync(group);

        // Assert
        AssertFailed(result, ErrorType.Conflict);
        Assert.Contains("התעמלות קרקע", result.Error);
        VerifyNothingSaved();
    }

    /// <summary>
    /// מכין את ה-Mocks למצב שבו כל הבדיקות עוברות. כל בדיקה משנה רק את מה שמיוחד לה.
    /// </summary>
    private void Arrange(CourseGroup group, User? member = null, List<CourseGroup>? sameDayGroups = null)
    {
        member ??= TestData.Member();

        _groups.Setup(r => r.GetByIdAsync(group.Id, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _categories.Setup(r => r.GetByIdAsync(group.CategoryId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(group.Category);
        _users.Setup(r => r.GetByIdAsync(member.Id, It.IsAny<CancellationToken>())).ReturnsAsync(member);
        _enrollments.Setup(r => r.HasActiveEnrollmentAsync(member.Id, group.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
        _enrollments.Setup(r => r.GetActiveGroupsOfUserOnDayAsync(
                        member.Id, It.IsAny<DayOfWeek>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(sameDayGroups ?? []);

        // אחרי השמירה ה-Service טוען את הרישום מחדש. כאן מחזירים את הרישום שנוסף, עם הקבוצה והמשתתפת
        Enrollment? added = null;
        _enrollments.Setup(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
                    .Callback<Enrollment, CancellationToken>((enrollment, _) =>
                    {
                        enrollment.User = member;
                        enrollment.CourseGroup = group;
                        added = enrollment;
                    })
                    .Returns(Task.CompletedTask);
        _enrollments.Setup(r => r.GetWithGroupAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => added);
    }

    private Task<Result<EnrollmentDto>> EnrollAsync(CourseGroup group)
        => _service.EnrollAsync(new CreateEnrollmentDto { CourseGroupId = group.Id }, MemberUser, CancellationToken.None);

    private static void AssertFailed(Result result, ErrorType expected)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(expected, result.ErrorType);
    }

    private void VerifyNothingSaved()
        => _enrollments.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
}
