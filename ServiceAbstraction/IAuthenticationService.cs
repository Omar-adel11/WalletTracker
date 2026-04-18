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
        Task<bool> CheckEmailExistence(string Email);
        Task<bool> ForgetPassword(string Email);
        Task<string> VerifyOtpAsync(string Email, string otp);
        Task<bool> resetPassword(string resetToken,string Email,string newPassword); //to use otp to reset password
        Task<bool> ChangePassword(string email,string oldPassword, string newPassword);
        
    }
}
