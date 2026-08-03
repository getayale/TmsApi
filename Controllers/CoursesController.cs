using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.DTOs;
using TmsApi.Services;

namespace TmsApi.Controllers;


[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService courseService,
    LinkGenerator linkGenerator) : ControllerBase
{


    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResponse<CourseResponseDto>),
        StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription(
        "Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var result =
            await courseService.GetCoursesAsync(request, ct);

        return Ok(result);
    }




    [HttpGet("{id:int}",
        Name = nameof(GetCourseById))]
    [ProducesResponseType(
        typeof(CourseDetailDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription(
        "Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetCourseById(
        int id,
        CancellationToken ct)
    {
        var course =
            await courseService.GetByIdAsync(id, ct);


        return course is null
            ? NotFound()
            : Ok(course);
    }





    [HttpPost]
    [ProducesResponseType(
        typeof(CourseResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription(
        "Creates a course with a unique code. Returns 409 if the course code already exists.")]
    public async Task<IActionResult> CreateCourse(
        CreateCourseRequest request,
        CancellationToken ct)
    {

        if (await courseService.CodeExistsAsync(
            request.Code,
            ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail =
                    $"A course with code '{request.Code}' already exists.",
                Status =
                    StatusCodes.Status409Conflict
            });
        }


        var result =
            await courseService.CreateAsync(request, ct);


        return CreatedAtAction(
            nameof(GetCourseById),
            new
            {
                id = result.Id
            },
            result);
    }





    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(CourseResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Update a course")]
    [EndpointDescription(
        "Updates an existing course.")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        UpdateCourseRequest request,
        CancellationToken ct)
    {
        var result =
            await courseService.UpdateAsync(
                id,
                request,
                ct);


        return result is null
            ? NotFound()
            : Ok(result);
    }





    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a course")]
    [EndpointDescription(
        "Deletes an existing course by ID.")]
    public async Task<IActionResult> DeleteCourse(
        int id,
        CancellationToken ct)
    {
        var deleted =
            await courseService.DeleteAsync(id, ct);


        return deleted
            ? NoContent()
            : NotFound();
    }
}