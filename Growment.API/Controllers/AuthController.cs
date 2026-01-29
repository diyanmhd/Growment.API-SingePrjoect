using Dapper;
using Growment.API.Common;
using Growment.API.Data;
using Growment.API.Models;
using Growment.API.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Growment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IJwtService _jwtService;
        private readonly DapperContext _context;

        public AuthController(
            AuthService authService,
            IRefreshTokenService refreshTokenService,
            IJwtService jwtService,
            DapperContext context)
        {
            _authService = authService;
            _refreshTokenService = refreshTokenService;
            _jwtService = jwtService;
            _context = context;
        }

        // ===================== REGISTER =====================
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        // ===================== LOGIN =====================
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var response = await _authService.LoginAsync(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        // ===================== REFRESH TOKEN =====================
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(ApiResponse.Fail("Refresh token is required"));

            // 1️⃣ Validate refresh token
            var isValid = await _refreshTokenService.ValidateAsync(request.RefreshToken);
            if (!isValid)
                return Unauthorized(ApiResponse.Fail("Invalid refresh token"));

            // 2️⃣ Get userId from refresh token
            var userId = await _refreshTokenService.GetUserIdAsync(request.RefreshToken);
            if (userId == null)
                return Unauthorized(ApiResponse.Fail("Invalid refresh token"));

            // 3️⃣ Revoke old refresh token
            await _refreshTokenService.RevokeAsync(request.RefreshToken);

            // 4️⃣ Get user details
            using var connection = _context.CreateConnection();
            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT UserId, Email, Role FROM Users WHERE UserId = @UserId",
                new { UserId = userId }
            );

            if (user == null)
                return Unauthorized(ApiResponse.Fail("User not found"));

            // 5️⃣ Generate new tokens
            var newAccessToken =
                _jwtService.GenerateToken(user.UserId, user.Email, user.Role);

            var newRefreshToken =
                await _refreshTokenService.CreateAndStoreAsync(user.UserId);

            return Ok(ApiResponse.Ok("Token refreshed", new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken,
                expiresIn = 3600
            }));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(ApiResponse.Fail("Refresh token is required"));

            // Revoke refresh token
            await _refreshTokenService.RevokeAsync(request.RefreshToken);

            return Ok(ApiResponse.Ok("Logged out successfully"));
        }
    }
}
