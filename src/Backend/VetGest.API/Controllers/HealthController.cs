using Microsoft.AspNetCore.Mvc;
using VetGest.Contracts.Common;

namespace VetGest.API.Controllers;

/// <summary>
/// Health check endpoint to validate API configuration and database connectivity.
/// Used for deployment verification and monitoring.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IWebHostEnvironment _environment;

    public HealthController(ILogger<HealthController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Check if the API is running and responsive.
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    public ActionResult<ApiResponse> Get()
    {
        _logger.LogInformation("Health check requested");
        return Ok(ApiResponse.Ok());
    }

    /// <summary>
    /// Check detailed system status including database connectivity.
    /// </summary>
    [HttpGet("detailed")]
    [Produces("application/json")]
    public ActionResult<ApiResponse<HealthStatusDto>> GetDetailed()
    {
        var status = new HealthStatusDto
        {
            Status = "Saudável",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = _environment.EnvironmentName
        };

        _logger.LogInformation("Detailed health check requested. Status: {Status}", status.Status);
        return Ok(ApiResponse<HealthStatusDto>.Ok(status));
    }
}
