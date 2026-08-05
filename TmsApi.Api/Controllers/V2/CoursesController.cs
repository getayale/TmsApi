using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Courses.Queries;
using TmsApi.Application.DTOs;


namespace TmsApi.Controllers.V2;


[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(
    IMediator mediator)
    : ControllerBase
{


    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {

        page = Math.Max(1, page);

        pageSize = Math.Clamp(pageSize, 1, 50);


        var result = await mediator.Send(
            new GetCoursesQuery(page, pageSize),
            ct);



        return Ok(new
        {
            data = result.Items,

            meta = new
            {
                result.TotalCount,
                result.Page,
                result.PageSize,

                totalPages =
                    (int)Math.Ceiling(
                        result.TotalCount /
                        (double)result.PageSize),

                hasNext =
                    result.Page <
                    Math.Ceiling(
                        result.TotalCount /
                        (double)result.PageSize),

                hasPrevious =
                    result.Page > 1
            },


            links = new
            {
                self =
                    $"/api/v2/courses?page={result.Page}&pageSize={result.PageSize}",


                next =
                    result.Page <
                    Math.Ceiling(
                        result.TotalCount /
                        (double)result.PageSize)
                    ?
                    $"/api/v2/courses?page={result.Page + 1}&pageSize={result.PageSize}"
                    :
                    null,


                prev =
                    result.Page > 1
                    ?
                    $"/api/v2/courses?page={result.Page - 1}&pageSize={result.PageSize}"
                    :
                    null,


                enroll = "/api/v2/enrollments"
            }
        });
    }



    // M7 Exercise 3 Step 8
    // Update course + invalidate HybridCache

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        [FromBody] UpdateCourseRequest request,
        CancellationToken ct)
    {

        var result = await mediator.Send(
            new UpdateCourseCommand(
                id,
                request.Code,
                request.Title,
                request.MaxCapacity),
            ct);



        if (!result)
        {
            return NotFound();
        }


        return NoContent();
    }
}