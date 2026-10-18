using AutoMapper;
using ChugimCenter.Application.Common;
using ChugimCenter.Application.DTOs.CourseGroups;
using ChugimCenter.Domain.Entities;

namespace ChugimCenter.Application.Mapping;

public class CourseGroupProfile : Profile
{
    public CourseGroupProfile()
    {
        // CategoryName מועתק אוטומטית מ-Category.Name (AutoMapper מחבר את השמות לבד)
        CreateMap<CourseGroup, CourseGroupDto>()
            .ForMember(d => d.InstructorName, o => o.MapFrom(s => s.Instructor.FullName))
            .ForMember(d => d.EndDate, o => o.MapFrom(s => CourseGroupSchedule.GetEndDate(s)))
            .ForMember(d => d.AvailableSpots, o => o.MapFrom(s => s.Capacity - s.EnrolledCount));

        // יצירה ועדכון: מועתקים רק השדות שבבקשה.
        // מזהה, יום בשבוע, מספר נרשמות, סטטוס וגרסה לא נמצאים בבקשה, ולכן לא נדרסים בעדכון
        CreateMap<CourseGroupRequestDto, CourseGroup>(MemberList.Source);
    }
}
