using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserRegistrationApi.Data;
using UserRegistrationApi.Models;
using UserRegistrationApi.Models.Dtos;
using UserRegistrationApi.Services;

namespace UserRegistrationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // 1. Full Dashboard (All Registrations)
        [Authorize(Roles = "admin")]
        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            var registrations = _db.Registrations
                .OrderByDescending(r => r.SubmittedAt)
                .ToList();

            return Ok(registrations);
        }

        // 2. Pending Only
        [Authorize(Roles = "admin")]
        [HttpGet("pending")]
        public IActionResult GetPendingRegistrations()
        {
            var pending = _db.Registrations
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.SubmittedAt)
                .ToList();

            return Ok(pending);
        }

        // 3. Approve or Reject (with Camunda task filtering)
        [Authorize(Roles = "admin")]
        [HttpPost("approve-reject/{id}")]
        public async Task<IActionResult> ApproveOrReject(
            int id,
            [FromBody] ApproveRejectDto dto,
            [FromServices] CamundaService camunda)
        {
            var registration = await _db.Registrations.FindAsync(id);

            if (registration == null)
                return NotFound(new { message = "Registration not found" });

            // Update registration status
            registration.Status = dto.IsApproved ? "Approved" : "Rejected";
            registration.ReviewedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Fetch tasks from Camunda
            var tasks = await camunda.GetTasksAsync();
            string? matchingTaskId = null;

            // Find task with matching registrationId variable
            foreach (var task in tasks)
            {
                string taskId = task["id"];
                var variables = await camunda.GetTaskVariablesAsync(taskId);

                if (variables.ContainsKey("registrationId") &&
                    Convert.ToInt32(variables["registrationId"]["value"]) == registration.Id)
                {
                    matchingTaskId = taskId;
                    break;
                }
            }

            if (matchingTaskId == null)
                return NotFound(new { message = "No matching Camunda task found for this registration" });

            // Complete task in Camunda
            await camunda.CompleteTaskAsync(matchingTaskId, dto.IsApproved);

            return Ok(new
            {
                message = $"Registration {(dto.IsApproved ? "approved" : "rejected")} successfully, workflow updated.",
                registration
            });
        }
    }
}
