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
using Microsoft.AspNetCore.Hosting;
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
        IUnitOfWork unitOfWork,
        IWebHostEnvironment _environment) : IAuthenticationService
    {
        public async Task<UserDTO> LogInAsync(UserLoginDTO userLoginDTO)
        {
            var user = await _userManager.FindByEmailAsync(userLoginDTO.Email);
            if (user is null) throw new UserNotFoundNullException();
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, userLoginDTO.Password);
            if (!isPasswordValid) throw new UnAuthorizedException("Invalid Email or password.");

            var token = await GenerateTokenAsync(user);
            return new UserDTO
            {
                Email = user.Email,
                UserName = user.UserName,
                PictureUrl = user.PictureUrl,
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

            user.PictureUrl = Helper.DocumentSettings.UploadFile(userSignUpDTO.file, _environment.WebRootPath, "images");
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
                PictureUrl = user.PictureUrl,
                Token = token
            };
        }
        public async Task<UserDTO?> GetUserAsync(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user is null) throw new UserNotFoundNullException();
            var token = await GenerateTokenAsync(user);
            return new UserDTO
            {
                Email = user.Email,
                UserName = user.UserName,
                PictureUrl = user.PictureUrl,
                Token = token
            };
        }

        public async Task<bool> CheckEmailExistence(string Email)
        {
            return await _userManager.FindByEmailAsync(Email) != null;
        }

        public async Task<string> ChangePassword(string email ,ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) throw new UserNotFoundNullException();
            var isMatch = await _userManager.CheckPasswordAsync(user, changePasswordDto.oldPassword);
            if (!isMatch) throw new CurrentPasswordBadRequestException();
            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.oldPassword, changePasswordDto.newPassword);
            if (!result.Succeeded)
                throw new RegisterationBadRequestException(result.Errors.Select(E => E.Description));
            return "Password changed successfully.";
        }

        public async Task<string> ForgetPassword(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);

            if (user is not null)
            {
                var otp = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");


                var emailModel = new Email
                {
                    To = Email,
                    Subject = "Your WalletTracker Reset Code",
                    Body = $"<h1>Reset Your Password</h1><p>Your 4-digit code is: <b>{otp}</b></p>"
                };

                await _emailService.SendEmailAsync(emailModel);
                return "If the email exists, a reset code has been sent.";
            }
            else
                throw new UserNotFoundNullException();
        }

        public async Task<string> VerifyOtpAsync(VerifyOTPDTO verifyOTPDTO)
        {
            var user = await _userManager.FindByEmailAsync(verifyOTPDTO.Email);
            if (user is null) throw new UserNotFoundNullException();

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", verifyOTPDTO.OTP);
            if (!isValid) throw new UnAuthorizedException("Invalid or expired OTP.");


            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }
        public async Task<string> resetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (user is null) throw new UserNotFoundNullException();

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDTO.ResetToken, resetPasswordDTO.Password );

            if (!result.Succeeded)
                throw new RegisterationBadRequestException(result.Errors.Select(e => e.Description));

            return "Password Reset successfuly";
        }

        public async Task<UserDTO?> UpdateUserAsync(string Email,UpdateUserDTO UpdateUserDTO)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user is null) throw new UserNotFoundNullException();
            user.UserName = UpdateUserDTO.UserName ?? user.UserName;
            if (UpdateUserDTO.file != null)
            {
                if (user.PictureUrl != null)
                {
                    Helper.DocumentSettings.DeleteFile(user.PictureUrl, _environment.WebRootPath, "images");
                }
                user.PictureUrl = Helper.DocumentSettings.UploadFile(UpdateUserDTO.file, _environment.WebRootPath, "images");
            }
            user.PhoneNumber = UpdateUserDTO.PhoneNumber ?? user.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new RegisterationBadRequestException(result.Errors.Select(E => E.Description));
            return new UserDTO
            {
                Email = user.Email,
                UserName = user.UserName,
                PictureUrl = user.PictureUrl,
                Token = await GenerateTokenAsync(user)
            };
        }


        private async Task<string> GenerateTokenAsync(User user)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.AddDays(
                double.Parse(_config["Jwt:DurationInDays"]!)
            ),
                signingCredentials: creds,
                claims: claims
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
