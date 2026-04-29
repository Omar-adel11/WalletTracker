using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Presentation.Attributes;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.ItemToBuyDTOs;

namespace Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Cache("")]
    public class ItemToBuyController(IServiceManager _serviceManager) : ControllerBase
    {
        private int userId => int.Parse(User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value)!;

        [HttpGet]
        public async Task<IActionResult> GetAllItems([FromQuery] int? PageNumber, int? PageSize)
        {
            var result = await _serviceManager.ItemToBuyService.GetAllItemsToBuyAsync(userId,PageNumber,PageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById([FromRoute] int id)
        {
            var result = await _serviceManager.ItemToBuyService.GetItemToBuyByIdAsync(id,userId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] CreateItemToBuyDTO dto)
        {
            var result = await _serviceManager.ItemToBuyService.AddItemAsync(userId, dto);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteItem([FromRoute]int id)
        {
            await _serviceManager.ItemToBuyService.DeleteItemAsync(id,userId);
            return NoContent();
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateItem([FromRoute] int id,[FromBody]UpdateItemToBuyDTO dto)
        {
            dto.Id = id;
            await _serviceManager.ItemToBuyService.UpdateItemAsync(userId,dto);
            return NoContent();
        }

        [HttpPost("{id}/save-money")]
        public async Task<IActionResult> SaveMoney([FromRoute] int id,[FromBody] SaveMoneyDTO dto)
        {
            dto.Id = id;
            var result = await _serviceManager.ItemToBuyService.SaveMoneyASync(userId, dto);
            return NoContent();
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompletePurchase([FromRoute] int id)
        {
            var result = await _serviceManager.ItemToBuyService.CompletePurchaseAsync(id, userId);
            return NoContent();
        }
    }
}
