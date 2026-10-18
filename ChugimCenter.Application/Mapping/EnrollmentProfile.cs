using AutoMapper;
using ChugimCenter.Application.DTOs.Enrollments;
using ChugimCenter.Application.DTOs.Users;
using ChugimCenter.Domain.Entities;

namespace ChugimCenter.Application.Mapping;

public class EnrollmentProfile : Profile
{
    public EnrollmentProfile()
    {
        // CourseGroupName מועתק אוטומטית מ-CourseGroup.Name
        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.CourseGroup.Category.Name))
            .ForMember(d => d.DayOfWeek, o => o.MapFrom(s => s.CourseGroup.DayOfWeek))
            .ForMember(d => d.StartTime, o => o.MapFrom(s => s.CourseGroup.StartTime));

        // תלמידה ברשימת התלמידות של קבוצה: פרטי הרישום ופרטי המשתתפת
        CreateMap<Enrollment, StudentDto>()
            .ForMember(d => d.EnrollmentId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.BirthDate, o => o.MapFrom(s => s.User.BirthDate))
            .ForMember(d => d.ParentName, o => o.MapFrom(s => s.User.ParentName))
            .ForMember(d => d.ParentPhone, o => o.MapFrom(s => s.User.ParentPhone));
    }
}
