using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.Auth;

namespace Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController(IServiceManager _serviceManger) : ControllerBase
    {

        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] UserLoginDTO loginDTO)
        {
            var result = await _serviceManger.AuthenticationService.LogInAsync(loginDTO);
            return Ok(result);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromForm] UserSignUpDTO signUpDTO)
        {
            var result = await _serviceManger.AuthenticationService.SignUpAsync(signUpDTO);
            return Ok(result);
        }

        [HttpGet("get-current-user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var result = await _serviceManger.AuthenticationService.GetUserAsync(email);
            return Ok(result);
        }

        [HttpGet("check-email-existence")]
        public async Task<IActionResult> CheckEmailExistence([FromQuery] string email)
        {
            var result = await _serviceManger.AuthenticationService.CheckEmailExistence(email);
            return Ok(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value; var result = await _serviceManger.AuthenticationService.ChangePassword(email, changePasswordDto);
            return Ok(result);
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] string email)
        {
            var result = await _serviceManger.AuthenticationService.ForgetPassword(email);
            return Ok(result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPDTO verifyOTP)
        {
            var result = await _serviceManger.AuthenticationService.VerifyOtpAsync(verifyOTP);
            return Ok(result);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
        {
            var result = await _serviceManger.AuthenticationService.resetPassword(resetPasswordDto);
            return Ok(result);
        }

        [HttpPatch("update-user")]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfo([FromForm] UpdateUserDTO updateUserInfo)
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value; var result = await _serviceManger.AuthenticationService.UpdateUserAsync(email,updateUserInfo);
            return Ok(result);
        }
    }
}
