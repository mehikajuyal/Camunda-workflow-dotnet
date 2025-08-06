using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserRegistrationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        // This endpoint can only be accessed by users with the "admin" role
        [Authorize(Roles = "admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminEndpoint()
        {
            return Ok("Welcome Admin");
        }

        // This endpoint can only be accessed by users with the "user" role
        [Authorize(Roles = "user")]
        [HttpGet("user-only")]
        public IActionResult UserEndpoint()
        {
            return Ok("Welcome User");
        }
    }
}