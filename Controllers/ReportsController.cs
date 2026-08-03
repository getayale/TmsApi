using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;

namespace TmsApi.Controllers;


[ApiController]
[Route("api/reports")]
public class ReportsController(
    IReportingService reportingService) : ControllerBase
{

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await reportingService
            .GetStudentsPageAsync(page, pageSize, ct);

        return Ok(result);
    }


    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopCourses(
        CancellationToken ct)
    {
        var result = await reportingService
            .GetTopCoursesAsync(ct);

        return Ok(result);
    }
}