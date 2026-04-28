using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceAbstraction.DTOs.CategoryDtos;
using Shared;

namespace ServiceAbstraction
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryDto>> GetAllCategoriesAsync(int? UserId = null, int? PageNumber = 1, int? PageSize = 5);
        Task<CategoryDto> GetCategoryByIdAsync(int userId,int id);
        Task<CategoryDto> CreateCategoryAsync(string name, int? UserId = null);
        Task DeleteCategoryAsync(int userId,int CategoryId);
        Task UpdateCategoryAsync(int userId, int CategoryId, string newName);
    }
}
