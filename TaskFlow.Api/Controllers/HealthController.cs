using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using TaskFlow.Api.Data;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public HealthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var canConnect = _context.Database.CanConnect();

        return Ok(new
        {
            status = "TaskFlow API is running!",
            database = canConnect ? "Yep Connected ✅" : "Error... Something went wrong."
        });
    }
}