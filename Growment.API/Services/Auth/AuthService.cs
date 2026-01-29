using Dapper;
using Growment.API.Common;
using Growment.API.Data;
using Growment.API.Models;
using System.Data;
using BCrypt.Net;

namespace Growment.API.Services.Auth
{
    public class AuthService
    {
        private readonly DapperContext _context;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthService(
            DapperContext context,
            IJwtService jwtService,
            IRefreshTokenService refreshTokenService)
        {
            _context = context;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
        }

        // ===================== REGISTER =====================
        public async Task<ApiResponse> RegisterAsync(UserRegisterRequest request)
        {
            try
            {
                using var connection = _context.CreateConnection();

                var emailExistsQuery =
                    "SELECT COUNT(1) FROM Users WHERE Email = @Email AND IsDeleted = 0";

                var exists = await connection.ExecuteScalarAsync<int>(
                    emailExistsQuery,
                    new { Email = request.Email }
                );

                if (exists > 0)
                    return ApiResponse.Fail("Email already registered");

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var parameters = new DynamicParameters();
                parameters.Add("@Email", request.Email);
                parameters.Add("@PasswordHash", passwordHash);
                parameters.Add("@FirstName", request.FirstName);
                parameters.Add("@LastName", request.LastName);
                parameters.Add("@PhoneNumber", request.PhoneNumber);

                await connection.ExecuteAsync(
                    "sp_CreateUser",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return ApiResponse.Ok("User registered successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail(ex.Message);
            }
        }

        // ===================== LOGIN (JWT + REFRESH TOKEN) =====================
        public async Task<ApiResponse> LoginAsync(UserLoginRequest request)
        {
            try
            {
                using var connection = _context.CreateConnection();

                var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_LoginUser",
                    new { Email = request.Email },
                    commandType: CommandType.StoredProcedure
                );

                if (user == null)
                    return ApiResponse.Fail("Invalid email or password");

                bool isPasswordValid =
                    BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

                if (!isPasswordValid)
                    return ApiResponse.Fail("Invalid email or password");

                // 🔐 Generate access token (JWT)
                var accessToken = _jwtService.GenerateToken(
                    user.UserId,
                    user.Email,
                    user.Role
                );

                // ♻️ Generate & store refresh token (delegated)
                var refreshToken =
                    await _refreshTokenService.CreateAndStoreAsync(user.UserId);

                return ApiResponse.Ok("Login successful", new
                {
                    accessToken = accessToken,
                    refreshToken = refreshToken,
                    expiresIn = 3600,
                    role = user.Role,
                    user = new
                    {
                        user.UserId,
                        user.Email,
                        user.FirstName,
                        user.LastName
                    }
                });
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail(ex.Message);
            }
        }
    }
}
