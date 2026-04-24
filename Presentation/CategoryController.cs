using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;

namespace Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoryController(IServiceManager _serviceManager) : ControllerBase
    {
        private int userId => int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value)!;
        [HttpGet]
        
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _serviceManager.CategoryService.GetAllCategoriesAsync(userId);
            return Ok(categories);
            
        }

        [HttpGet("{id}")]

        public async Task<IActionResult> GetCategory([FromRoute]int id)
        {
            var category = await _serviceManager.CategoryService.GetCategoryByIdAsync(id);
            return Ok(category);

        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] string name)
        {
            var result = await _serviceManager.CategoryService.CreateCategoryAsync(name,userId);
            return Ok(result);
        }

        [HttpDelete("delete-Category/{id}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id)
        {
            await _serviceManager.CategoryService.DeleteCategoryAsync(id);
            return NoContent();

        }

        [HttpPut("Update-category/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] string name)
        {
            await _serviceManager.CategoryService.UpdateCategoryAsync(id, name);
            return NoContent();
        }
    } 
}
