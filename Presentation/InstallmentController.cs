using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.BudgetDTOs;
using ServiceAbstraction.DTOs.InstallmentsDTOs;

namespace Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstallmentController(IServiceManager _serviceManager) : ControllerBase
    {
        private int userId => int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        [HttpGet]
        public async Task<IActionResult> GetInstallments()
        {
            var result = await _serviceManager.InstallmentsService.GetAllInstallmentsAsync(userId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInstallmentById([FromRoute] int id)
        {
            var result = await _serviceManager.InstallmentsService.GetInstallmentsByIdAsync(id,userId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInstallment([FromBody] CreateInstallmentDTO dto)
        {
            var result = await _serviceManager.InstallmentsService.CreateInstallmentAsync(userId,dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInstallment([FromRoute]int id)
        {
            await _serviceManager.InstallmentsService.DeleteInstallmentAsync(id,userId);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateInstallment([FromRoute]int id, [FromBody] UpdateInstallmentDTO dto)
        {
            await _serviceManager.InstallmentsService.UpdateInstallmentAsync(userId, id, dto);
            return NoContent();
        }

        [HttpPost("pay-installment/{id}")]
        public async Task<IActionResult> PayInstallment([FromRoute] int id, [FromBody] PayInstallmentDTO dto)
        {
            var result = await _serviceManager.InstallmentsService.payInstallmentAsync(id,dto);
            return Ok(result);
        }
    }
}
