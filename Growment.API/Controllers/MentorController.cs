using Growment.API.Common;
using Growment.API.Models.Mentor;
using Growment.API.Services.Mentor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Growment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MentorController : ControllerBase
    {
        private readonly IMentorService _mentorService;

        public MentorController(IMentorService mentorService)
        {
            _mentorService = mentorService;
        }

        // ================= USER =================

        [Authorize(Roles = "User")]
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyForMentor([FromBody] ApplyMentorRequest request)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await _mentorService.ApplyForMentorAsync(userId, request);

            return Ok(ApiResponse.Ok("Mentor application submitted successfully"));
        }


        // ================= ADMIN =================

        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApplications()
        {
            var applications = await _mentorService.GetPendingApplicationsAsync();

            return Ok(ApiResponse.Ok("Pending mentor applications fetched", applications));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{applicationId}/review")]
        public async Task<IActionResult> ReviewApplication(
            int applicationId,
            [FromBody] ReviewMentorApplicationRequest request)
        {
            int adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            await _mentorService.ReviewApplicationAsync(
                applicationId,
                request.Status,
                adminUserId
            );

            return Ok(ApiResponse.Ok("Mentor application reviewed successfully"));
        }
    }
}
