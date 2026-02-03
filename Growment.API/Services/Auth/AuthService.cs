using Dapper;
using Growment.API.Common;
using Growment.API.Data;
using Growment.API.Models;
using Growment.API.Services.Common;
using System.Data;
using BCrypt.Net;

namespace Growment.API.Services.Auth
{
    public class AuthService
    {
        private readonly DapperContext _context;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly EmailService _emailService;

        public AuthService(
            DapperContext context,
            IJwtService jwtService,
            IRefreshTokenService refreshTokenService,
            EmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _emailService = emailService;
        }

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

                var accessToken = _jwtService.GenerateToken(
                    user.UserId,
                    user.Email,
                    user.Role
                );

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

        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public async Task<ApiResponse> GenerateChangePasswordOtpAsync(int userId, string email)
        {
            try
            {
                using var connection = _context.CreateConnection();
                const string purpose = "CHANGE_PASSWORD";

                await connection.ExecuteAsync(
                    @"UPDATE UserOtps 
                      SET IsUsed = 1 
                      WHERE UserId = @UserId AND Purpose = @Purpose",
                    new { UserId = userId, Purpose = purpose }
                );

                var otp = GenerateOtp();

                await connection.ExecuteAsync(
                    @"INSERT INTO UserOtps (UserId, OtpCode, Purpose, ExpiryAt)
                      VALUES (@UserId, @Otp, @Purpose, @Expiry)",
                    new
                    {
                        UserId = userId,
                        Otp = otp,
                        Purpose = purpose,
                        Expiry = DateTime.UtcNow.AddMinutes(5)
                    }
                );

                await _emailService.SendOtpEmailAsync(email, otp);

                return ApiResponse.Ok("OTP sent to your email");
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail(ex.Message);
            }
        }

        public async Task<ApiResponse> ChangePasswordWithOtpAsync(OtpChangePasswordRequest request)
        {
            try
            {
                using var connection = _context.CreateConnection();

                var otpData = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT TOP 1 *
                      FROM UserOtps
                      WHERE OtpCode = @Otp
                        AND IsUsed = 0
                        AND ExpiryAt > GETUTCDATE()",
                    new { Otp = request.Otp }
                );

                if (otpData == null)
                    return ApiResponse.Fail("Invalid or expired OTP");

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                await connection.ExecuteAsync(
                    @"UPDATE Users
                      SET PasswordHash = @PasswordHash, UpdatedAt = GETUTCDATE()
                      WHERE UserId = @UserId",
                    new
                    {
                        PasswordHash = passwordHash,
                        UserId = otpData.UserId
                    }
                );

                await connection.ExecuteAsync(
                    @"UPDATE UserOtps
                      SET IsUsed = 1
                      WHERE Id = @Id",
                    new { Id = otpData.Id }
                );

                return ApiResponse.Ok("Password changed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail(ex.Message);
            }
        }
    }
}
