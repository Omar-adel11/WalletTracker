using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Exceptions.AuthExceptions;
using Domain.Exceptions.NullReferenceException;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.CategoryDtos;
using ServiceAbstraction.DTOs.TransactionDtos;
using Shared;

namespace Service
{
    public class CategoryService(IUnitOfWork _unitOfWork,IMapper _mapper) : ICategoryService
    {
        private IGenericRepository<Category> _repo = _unitOfWork.Repository<Category>();
        public async Task<PagedResult<CategoryDto>> GetAllCategoriesAsync(int? UserId = null, int? PageNumber = 1, int? PageSize = 5)
        {
            var categories = await _repo.GetAsyncFilteredWithPaginate(c => c.UserId == null || c.UserId == UserId,
                                                                        t => t.CreatedAt,
                                                                        PageNumber,
                                                                        PageSize   
                                                                        );
            if (!categories.Items.Any()) throw new EntityNotFoundException("category");
            var categoryDtos = _mapper.Map<PagedResult<CategoryDto>>(categories);
            return categoryDtos;
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int userId,int id)
        {
            var category = await GetAndAuthorize(userId, id);
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

        public async Task DeleteCategoryAsync(int userId, int CategoryId)
        {
            var category = await GetAndAuthorize(userId, CategoryId);
            _repo.Delete(category);
            await _unitOfWork.CompleteAsync();
        }

        

        public async Task UpdateCategoryAsync(int userId,int CategoryId, string newName)
        {
            var category = await GetAndAuthorize(userId, CategoryId);

            category.Name = newName;
            category.UpdatedAt = DateTimeOffset.UtcNow;
            _repo.Update(category);
            await _unitOfWork.CompleteAsync();

        }

        private async Task<Category> GetAndAuthorize(int userId, int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category is null) throw new EntityNotFoundException("Category");
            if (category.UserId != userId && category.UserId is not null) throw new UnAuthorizedException("you are not authorized to get this Category");
            return category;
        }
    }
}
