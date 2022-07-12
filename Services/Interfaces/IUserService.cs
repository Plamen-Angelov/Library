using Common.Models.InputDTOs;
using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;

namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterAsync(UserEntity input);

        Task<LoginUserWithRolesDto> LoginAsync(LoginUserDto user);

        Task CreateCallbackUriAsync(ForgotPasswordDto forgotPasswordDto);

        Task SaveNewPasswordAsync(ResetPasswordDto resetPasswordDto);

        Task<int> GetCountOfAllReadersAsync();
    }
}
