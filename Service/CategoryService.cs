using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Exceptions.NullReferenceException;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.CategoryDtos;

namespace Service
{
    public class CategoryService(IUnitOfWork _unitOfWork,IMapper _mapper) : ICategoryService
    {
        private IGenericRepository<Category> _repo = _unitOfWork.Repository<Category>();
        public async Task<ICollection<CategoryDto>> GetAllCategoriesAsync(int? UserId = null)
        {
            var categories = await _repo.GetAsync(c => c.UserId == null || c.UserId == UserId);
            var categoryDtos = _mapper.Map<ICollection<CategoryDto>>(categories);
            return categoryDtos;
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category is null) throw new CategoryNullException(id);
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return categoryDto;
        }
        public async Task<CategoryDto> CreateCategoryAsync(string name, int? UserId = null)
        {
            var category = new Category {
                UserId = UserId ,
                Name = name ,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _repo.AddAsync(category);
            await _unitOfWork.CompleteAsync();
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return categoryDto;
        }

        public async Task DeleteCategoryAsync(int CategoryId)
        {
            var category = await _repo.GetByIdAsync(CategoryId);
            if (category is null) throw new CategoryNullException(CategoryId);

            _repo.Delete(category);
            await _unitOfWork.CompleteAsync();
        }

        

        public async Task UpdateCategoryAsync(int CategoryId, string newName)
        {
            var category = await _repo.GetByIdAsync(CategoryId);
            if (category is null) throw new CategoryNullException(CategoryId);

            category.Name = newName;
            category.UpdatedAt = DateTimeOffset.UtcNow;
            _repo.Update(category);
            await _unitOfWork.CompleteAsync();

        }
    }
}
