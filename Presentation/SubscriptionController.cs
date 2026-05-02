using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.SubsriptionDTO;

namespace Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController(IServiceManager _serviceManager) : ControllerBase
    {
        private int UserId => int.Parse(
            User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);

        // Returns client_secret → frontend uses it with Paymob SDK
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentDTO dto)
        {
            var result = await _serviceManager.PaymentProvider
                .InitiatePaymentAsync(UserId, dto);
            return Ok(result);
        }

        // Paymob calls this after payment — NO [Authorize] here!
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {

            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();
           
            // Paymob sends the HMAC in this header
            var hmacSignature = Request.Query["hmac"].ToString();

            await _serviceManager.SubscriptionService
                .HandleWebhookAsync(rawBody, hmacSignature);

            return Ok(); 
        }
        
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var status = await _serviceManager.SubscriptionService
                .GetSubscriptionStatusAsync(UserId);
            return Ok(status);
        }
    }
}
