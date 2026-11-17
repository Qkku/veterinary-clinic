
using GroomingSalon.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly ScheduleService _scheduleService;

    public ScheduleController(ScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableSlots(
        DateTime date,
        uint serviceId,
        uint? groomerId)
    {
        try
        {
            var slots = await _scheduleService.GetAvailableSlots(
                date,
                serviceId,
                groomerId
            );

            return Ok(new
            {
                Date = date.ToString("yyyy-MM-dd"),
                ServiceId = serviceId,
                Slots = slots
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
