using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Exceptions.AuthExceptions;
using Domain.Exceptions.BadRequestException;
using Domain.Exceptions.NullReferenceException;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.Auth;
using ServiceAbstraction.Helper.Email;

namespace Service
{
    public class AuthenticationService(IMapper _mapper,
        UserManager<User> _userManager,
        IConfiguration _config,
        IEmailService _emailService,
        IUnitOfWork unitOfWork) : IAuthenticationService
    {
        public async Task<UserDTO> LogInAsync(UserLoginDTO userLoginDTO)
        {
            var user = await _userManager.FindByEmailAsync(userLoginDTO.Email);
            if (user is null) throw new UserNotFoundNullException();
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, userLoginDTO.Password);
            if(!isPasswordValid) throw new UnAuthorizedException("Invalid Email or password.");

            var token = await GenerateTokenAsync(user);
            return new UserDTO
            {
                Email = user.Email,
                UserName = user.UserName,
                Token = token
            };

        }

        public async Task<UserDTO> SignUpAsync(UserSignUpDTO userSignUpDTO)
        {
            var IsEmailExist = await CheckEmailExistence(userSignUpDTO.Email);
            if (IsEmailExist) throw new EmailExistException();
            var IsUserNameExist = await _userManager.FindByNameAsync(userSignUpDTO.UserName) != null;
            if (IsUserNameExist) throw new UserNameExistException();
            var user = _mapper.Map<User>(userSignUpDTO);
            var result = await _userManager.CreateAsync(user, userSignUpDTO.Password);
            if (!result.Succeeded)
                throw new RegisterationBadRequestException(result.Errors.Select(E => E.Description));

            var defaultWallet = new Wallet
            {
                UserId = user.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await unitOfWork.Repository<Wallet>().AddAsync(defaultWallet);
            await unitOfWork.CompleteAsync();

            var token = await GenerateTokenAsync(user);
            return new UserDTO
            {
                Email = user.Email,
                UserName = user.UserName,
                Token = token
            };
        }
        public async Task<UserDTO?> GetUserAsync(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if(user is null) throw new UserNotFoundNullException();
            var token = await GenerateTokenAsync(user);
            return new UserDTO
            {
                Email = user.Email,
                UserName = user.UserName,
                Token = token
            };
        }

        public async Task<bool> CheckEmailExistence(string Email)
        {
            return await _userManager.FindByEmailAsync(Email) != null;
        }

        public async Task<bool> ChangePassword(string Email,string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user is null) throw new UserNotFoundNullException();
            var isMatch = await _userManager.CheckPasswordAsync(user, oldPassword);
            if (!isMatch) throw new CurrentPasswordBadRequestException();
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!result.Succeeded)
                throw new RegisterationBadRequestException(result.Errors.Select(E => E.Description));
            return true;
        }

        public async Task<bool> ForgetPassword(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
           
            if (user is null) throw new UserNotFoundNullException();

            
            var otp = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            

            var emailModel = new Email
            {
                To = Email,
                Subject = "Your WalletTracker Reset Code",
                Body = $"<h1>Reset Your Password</h1><p>Your 4-digit code is: <b>{otp}</b></p>"
            };

            await _emailService.SendEmailAsync(emailModel);
            return true;
        }

        public async Task<string> VerifyOtpAsync(string Email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user is null) throw new UserNotFoundNullException();

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", otp);
            if (!isValid) throw new UnAuthorizedException("Invalid or expired OTP.");

            
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }
        public async Task<bool> resetPassword(string resetToken, string Email, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user is null) throw new UserNotFoundNullException();

            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!result.Succeeded)
                throw new RegisterationBadRequestException(result.Errors.Select(e => e.Description));

            return true;
        }

        private async Task<string> GenerateTokenAsync(User user)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            var roles = await _userManager.GetRolesAsync(user); 
            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.AddDays(
                double.Parse(_config["Jwt:DurationInMinutes"]!)
            ),
                signingCredentials: creds,
                claims: claims
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
