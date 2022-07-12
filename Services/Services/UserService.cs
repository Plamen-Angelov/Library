using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Text;
using log4net;
using Common.Models.InputDTOs;
using DataAccess.Entities;
using DataAccess.Enums;
using Repositories.Interfaces;
using Repositories.Mappers;
using Services.EmailSender;
using Services.Interfaces;

using static Common.ExceptionMessages;

namespace Services.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserEntity> userManager;
        private readonly IConfiguration configuration;
        private readonly IMailSender mailSender;
        private readonly IUserRepository userRepository;
        private readonly ILog log = LogManager.GetLogger(typeof(UserService));

        public UserService(UserManager<UserEntity> userManager, IConfiguration configuration, IMailSender mailSender, IUserRepository userRepository)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.mailSender = mailSender;
            this.userRepository = userRepository;
        }

        public async Task<IdentityResult> RegisterAsync(UserEntity user)
        {
            var roleUser = Role.Reader.ToString();

            var createdResult = await userManager.CreateAsync(user, user.PasswordHash);

            if (!createdResult.Succeeded)
            {
                return createdResult;
            }

            /* Uncomment if you want to register a librarian.
            var roleLibrarian = Role.Librarian.ToString();
            await userManager.AddToRoleAsync(user, roleLibrarian); */

             /* Uncomment if you want to register an admin.
            var roleAdmin = Role.Admin.ToString();
            await userManager.AddToRoleAsync(user, roleAdmin); */

            var result = await userManager.AddToRoleAsync(user, roleUser);
            log.Info("User registration was successful");
            return result;
        }

        public async Task<LoginUserWithRolesDto> LoginAsync(LoginUserDto user)
        {
            var existingUser = await userManager.FindByEmailAsync(user.Email);

            if (existingUser is null)
            {
                return null!;
            }

            var isCorrect = await userManager.CheckPasswordAsync(existingUser, user.Password);

            if (!isCorrect)
            {
                return null!;
            }

            var roles = await userManager.GetRolesAsync(existingUser);
            log.Info("User is logged in.");

            return Mapper.ToLoginUserWithRoles(existingUser, roles);
        }

        public async Task CreateCallbackUriAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await userManager.FindByEmailAsync(forgotPasswordDto.Email);

            if (user is null)
            {
                return;
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validToken = WebEncoders.Base64UrlEncode(encodedToken);

            var uri = $"{configuration["HostUrl"]}/reset-password?email={forgotPasswordDto.Email}&token={validToken}";

            //ok
            //var param = new Dictionary<string, string>()
            //{
            //    { "email", forgotPasswordDto.Email },
            //    { "token", validToken },
            //};

            //var callbackUri = QueryHelpers.AddQueryString($"{configuration["HostUrl"]}/reset-password", param);

            await mailSender.SendEmailAsync(forgotPasswordDto.Email, "Reset password", "Follow the instructions to reset your password", 
                $"<p>To reset your password <a href='{ uri }'>Click here</a></p>");

            log.Info("Reset password email is sent");
        }

        public async Task SaveNewPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await userManager.FindByEmailAsync(resetPasswordDto.Email);

            if (user is null)
            {
                return;
            }

            var decodedToken = WebEncoders.Base64UrlDecode(resetPasswordDto.Token);
            var normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await userManager.ResetPasswordAsync(user, normalToken, resetPasswordDto.Password);

            if (!result.Succeeded)
            {
                log.Error($"SaveNewPassword method throws exception {RESET_PASSWORD_FAILED}.");
                throw new ArgumentException(RESET_PASSWORD_FAILED);
            }

            log.Info("Reset password was successfully.");
        }

        public async Task<int> GetCountOfAllReadersAsync()
        {
            var allReadersCount = await this.userRepository.GetCountOfAllReadersAsync();

            return allReadersCount;
        }
    }
}
