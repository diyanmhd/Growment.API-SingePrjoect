using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Growment.API.Controllers
{
    [ApiController]
    [Route("api/role-test")]
    public class RoleTestController : ControllerBase
    {
        [Authorize]
        [HttpGet("any-user")]
        public IActionResult AnyUser()
        {
            return Ok("Any logged-in user can access this");
        }

        [Authorize(Roles = "User")]
        [HttpGet("user-only")]
        public IActionResult UserOnly()
        {
            return Ok("User role access confirmed");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok("Admin role access confirmed");
        }
    }
}
