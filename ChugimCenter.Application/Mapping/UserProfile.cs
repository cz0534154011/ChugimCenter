using AutoMapper;
using ChugimCenter.Application.Common;
using ChugimCenter.Application.DTOs.Auth;
using ChugimCenter.Application.DTOs.Users;
using ChugimCenter.Domain.Entities;

namespace ChugimCenter.Application.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // ב-UserDto אין PasswordHash, ולכן ה-hash לעולם לא יוצא מהשרת
        CreateMap<User, UserDto>();

        CreateAccountMap<RegisterDto>();
        CreateAccountMap<CreateInstructorDto>();
    }

    /// <summary>
    /// מיפוי משותף ליצירת חשבון (הרשמה ויצירת מדריכה), כדי לא לכפול אותו.
    /// האימייל נשמר בצורה אחידה. הסיסמה לא מועתקת: השירות מצפין אותה בנפרד.
    /// התפקיד נקבע בשירות, ולא מהבקשה.
    /// </summary>
    private void CreateAccountMap<TSource>() where TSource : AccountRequestDto
        => CreateMap<TSource, User>(MemberList.Source)
            .ForMember(d => d.Email, o => o.MapFrom(s => EmailNormalizer.Normalize(s.Email)))
            .ForSourceMember(s => s.Password, o => o.DoNotValidate());
}
