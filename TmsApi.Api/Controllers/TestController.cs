using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{

    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine(
            "STEP 1: Building query");

        var query = context.Students
            .Where(s => s.GPA >= 3.0m);


        Console.WriteLine(
            "STEP 2: Adding sorting");

        var orderedQuery = query
            .OrderBy(s => s.Name);


        Console.WriteLine(
            "STEP 3: Executing query");

        var results = orderedQuery.ToList();


        Console.WriteLine(
            "STEP 4: Finished");


        return Ok(results);
    }
}