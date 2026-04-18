using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceAbstraction.DTOs.CategoryDtos;

namespace ServiceAbstraction
{
    public interface ICategoryService
    {
        Task<ICollection<CategoryDto>> GetAllCategoriesAsync(int? UserId = null);
        Task<CategoryDto> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(string name, int? UserId = null);
        Task DeleteCategoryAsync(int CategoryId);
        Task UpdateCategoryAsync(int CategoryId, string newName);
    }
}
