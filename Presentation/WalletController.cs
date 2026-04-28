using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Entities.Struct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.WalletsDtos;

namespace Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletController(IServiceManager _serviceManager) : ControllerBase
    {
        private int userId => int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value)!;
        [HttpGet]
        public async Task<IActionResult> GetAllWallets()
        {
            var wallets = await _serviceManager.WalletService.GetAllWalletsAsync(userId);
            return Ok(wallets);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWalletById([FromRoute] int id)
        {
            var wallet = await _serviceManager.WalletService.GetWalletByIdAsync(userId,id);
            return Ok(wallet);
        }
        [HttpGet("{id}/balance")] 
        public async Task<IActionResult> GetWalletBalance([FromRoute] int id)
        {
            var balance = await _serviceManager.WalletService.GetBalanceAsync(userId, id);
            return Ok(balance);
        }

        [HttpPost("CreateWallet")]
        public async Task<IActionResult> CreateWallet([FromBody] CreateWalletDTO dto)
        {
            dto.UserId = userId;
            var wallet = await _serviceManager.WalletService.CreateWalletAsync(dto);
            return Ok(wallet);

        }

        [HttpPatch("Deposit/{walletid}")]
        public async Task<IActionResult> Deposit(int walletid, [FromBody] WalletTransactionRequestDTO dto)
        {
            await _serviceManager.WalletService.DepositAsync(userId, walletid, dto.Amount, dto.Source);
            return NoContent();
        }
        [HttpPatch("Withdraw/{walletid}")]
        public async Task<IActionResult> Withdraw(int walletid, [FromBody] WalletTransactionRequestDTO dto)
        {
            await _serviceManager.WalletService.WithdrawAsync(userId, walletid, dto.Amount, dto.Source);
            return NoContent();
        }

        [HttpPatch("transaction-between-wallet/{fromWalletId}")]
        public async Task<IActionResult> TransactionBetweenWallet( int fromWalletId, [FromBody] WalletTransactionRequestDTO dto)
        {
            await _serviceManager.WalletService.TransactionBetweenWalletAsync(userId, fromWalletId,dto.ToWalletId.Value,dto.ToUserName, dto.Amount, dto.Source);
            return NoContent();
        }

    }
}
