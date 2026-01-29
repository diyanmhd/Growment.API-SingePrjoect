using Dapper;
using Growment.API.Data;
using System.Security.Cryptography;

namespace Growment.API.Services.Auth
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly DapperContext _context;

        public RefreshTokenService(DapperContext context)
        {
            _context = context;
        }

        public async Task<string> CreateAndStoreAsync(int userId)
        {
            var refreshToken = GenerateSecureToken();

            using var connection = _context.CreateConnection();

            var sql = @"
                INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, IsRevoked, CreatedAt)
                VALUES (@UserId, @Token, @ExpiresAt, 0, GETDATE())
            ";

            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            return refreshToken;
        }

        public async Task<bool> ValidateAsync(string refreshToken)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(1)
                FROM RefreshTokens
                WHERE Token = @Token
                  AND IsRevoked = 0
                  AND ExpiresAt > GETDATE()
            ";

            var count = await connection.ExecuteScalarAsync<int>(sql, new
            {
                Token = refreshToken
            });

            return count > 0;
        }

        public async Task<int?> GetUserIdAsync(string refreshToken)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT UserId
                FROM RefreshTokens
                WHERE Token = @Token
                  AND IsRevoked = 0
                  AND ExpiresAt > GETDATE()
            ";

            return await connection.QueryFirstOrDefaultAsync<int?>(sql, new
            {
                Token = refreshToken
            });
        }

        public async Task RevokeAsync(string refreshToken)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                UPDATE RefreshTokens
                SET IsRevoked = 1
                WHERE Token = @Token
            ";

            await connection.ExecuteAsync(sql, new
            {
                Token = refreshToken
            });
        }

        private string GenerateSecureToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
