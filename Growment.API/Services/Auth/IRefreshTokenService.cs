namespace Growment.API.Services.Auth
{
    public interface IRefreshTokenService
    {
        Task<string> CreateAndStoreAsync(int userId);
        Task<bool> ValidateAsync(string refreshToken);
        Task<int?> GetUserIdAsync(string refreshToken);
        Task RevokeAsync(string refreshToken);
    }
}
