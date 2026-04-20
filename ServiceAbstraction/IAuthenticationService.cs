using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceAbstraction.DTOs.Auth;

namespace ServiceAbstraction
{
    public interface IAuthenticationService
    {
        Task<UserDTO> LogInAsync(UserLoginDTO userLoginDTO);
        Task<UserDTO> SignUpAsync(UserSignUpDTO userLoginDTO);
        Task<UserDTO?> GetUserAsync(string Email);
        Task<UserDTO?> UpdateUserAsync(string email, UpdateUserDTO UpdateUserDTO);
        Task<bool> CheckEmailExistence(string Email);
        Task<string> ForgetPassword(string Email);
        Task<string> VerifyOtpAsync(VerifyOTPDTO verifyOTPDTO);
        Task<string> resetPassword(ResetPasswordDTO resetPasswordDTO); //to use otp to reset password
        Task<string> ChangePassword(string email,ChangePasswordDto changePasswordDto);
        
    }
}
