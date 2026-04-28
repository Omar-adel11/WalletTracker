using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.TransactionDtos;

namespace Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionController(IServiceManager _serviceManager) : ControllerBase
    {
        private int userId => int.Parse(User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value)!;
        [HttpGet]
        public async Task<IActionResult> GetTransactions( [FromQuery] int walletId, int PageSize, int PageNumber)
        {
            var result = await _serviceManager.TransactionService.GetTransactionByWalletAsync(userId,walletId,PageNumber, PageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById([FromRoute] int id)
        {
            var result = await _serviceManager.TransactionService.GetTransactionByIdAsync(userId, id);
            return Ok(result);
        }

        [HttpPost("create-transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDTO dto)
        {
            var result = await _serviceManager.TransactionService.CreateTransactionAsync(userId, dto);
            return Ok(result);
        }

        [HttpPut("update-transactio/{id}")]
        public async Task<IActionResult> UpdateTransaction([FromRoute] int id, [FromBody] UpdateTransactionDTO dto)
        {
            await _serviceManager.TransactionService.UpdateTransactionAsync(userId, dto);
            return NoContent();
        }

        [HttpDelete("delete-transaction/{id}")]
        public async Task<IActionResult> DeleteTransaction([FromRoute] int id)
        {
            await _serviceManager.TransactionService.DeleteTransactionAsync(userId, id);
            return NoContent();
        }
    }
}
