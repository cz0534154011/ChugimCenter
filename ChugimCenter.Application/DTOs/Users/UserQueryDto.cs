using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Domain.Enums;

namespace ChugimCenter.Application.DTOs.Users;

/// <summary>סינון רשימת המשתמשות (למנהלת).</summary>
public class UserQueryDto : PagingQuery
{
    public UserRole? Role { get; set; }
}
