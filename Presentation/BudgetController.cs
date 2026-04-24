using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.BudgetDTOs;

namespace Presentation {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BudgetController(IServiceManager _serviceManager) : ControllerBase
    {
        private int userId => int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);
        [HttpGet]
        public async Task<IActionResult> GetBugdets()
        {
            var result = await _serviceManager.BudgetService.GetBudgetsByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBugdetById([FromRoute] int id)
        {
            var result = await _serviceManager.BudgetService.GetBudgetAsync(id,userId);
            return Ok(result);
        }

        [HttpPost("create-budget")]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDTO dto)
        {
            dto.UserId = userId;
            var result = await _serviceManager.BudgetService.CreateBudgetAsync(dto);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteBudget([FromRoute] int id)
        {
            await _serviceManager.BudgetService.DeleteBudgetAsync(id, userId);
            return NoContent(); 
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBudget(int id, [FromBody] UpdateBudgetDTO dto)
        {
            dto.Id = id;
            await _serviceManager.BudgetService.UpdateBudgetAsync(userId, dto);
            return NoContent();
        }
    }
}
