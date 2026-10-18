using AutoMapper;
using ChugimCenter.Application.DTOs.Categories;
using ChugimCenter.Domain.Entities;

namespace ChugimCenter.Application.Mapping;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>();

        // יצירה ועדכון: רק השם מגיע מהבקשה.
        // MemberList.Source: בודקים שכל שדה בבקשה מופה, ושאר שדות הישות לא נוגעים בהם
        CreateMap<CategoryRequestDto, Category>(MemberList.Source);
    }
}
