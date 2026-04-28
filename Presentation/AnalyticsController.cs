using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;

namespace Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController(IServiceManager _serviceManager) : ControllerBase
    {
        int userId => int.Parse(User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)!.Value);

        [HttpGet("dashboard/{walletId}")]
        public async Task<IActionResult> GetDashboard([FromRoute] int walletId, [FromQuery] DateTimeOffset? from, DateTimeOffset? to)
        {
            var result = await _serviceManager.AnalyticsService.GetDashboardAsync(userId, walletId, from, to);
            return Ok(result);
        }

        [HttpGet("category-spending/{walletId}")]
        public async Task<IActionResult> GetCategorySpending([FromRoute] int walletId, [FromQuery] DateTimeOffset from, DateTimeOffset to)
        {
            var result = await _serviceManager.AnalyticsService.GetCategorySpendingsAsync(userId, walletId, from, to);
            return Ok(result);
        }


        [HttpGet("monthly-trends/{walletId}")]
        public async Task<IActionResult> GetMonthlyTrends(
            [FromRoute] int walletId,
            [FromQuery] int monthsBack = 6)
        {
            var result = await _serviceManager.AnalyticsService
                .GetMonthlyTrendsAsync(userId, walletId, monthsBack);
            return Ok(result);
        }

    }
}
