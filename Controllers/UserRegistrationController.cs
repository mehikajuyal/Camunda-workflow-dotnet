using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserRegistrationApi.Data;
using UserRegistrationApi.Models;
using UserRegistrationApi.Services;

namespace UserRegistrationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserRegistrationController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UserRegistrationController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize(Roles = "user")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitRegistration([FromBody] UserRegistration request, [FromServices] CamundaService camunda)
        {
            request.Status = "Pending";
            request.SubmittedAt = DateTime.UtcNow;

            _db.Registrations.Add(request);
            await _db.SaveChangesAsync();

            // Trigger Camunda process
            //await camunda.StartProcessAsync(request.Id, request.Name);

            return Ok(new { message = "Registration submitted successfully." });
        }


    }
}
