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

        // User submits registration and triggers Camunda workflow
        [Authorize(Roles = "user")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitRegistration(
            [FromBody] UserRegistration request,
            [FromServices] CamundaService camunda)
        {
            // Save registration data in DB
            request.Status = "Pending";
            request.SubmittedAt = DateTime.UtcNow;

            _db.Registrations.Add(request);
            await _db.SaveChangesAsync();

            // Trigger Camunda process with registrationId and name
            await camunda.StartProcessAsync(request.Id, request.Name);

            return Ok(new
            {
                message = "Registration submitted successfully and workflow started.",
                registrationId = request.Id
            });
        }
    }
}
