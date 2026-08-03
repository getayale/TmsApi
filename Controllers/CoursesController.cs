using Microsoft.AspNetCore.Mvc;
using TmsApi.DTOs;
using TmsApi.Services;


namespace TmsApi.Controllers;


[ApiController]
[Route("api/courses")]
public class CoursesController(
    ICourseService courseService) : ControllerBase
{


    // GET ALL
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var result =
            await courseService.GetCoursesAsync(request, ct);

        return Ok(result);
    }



    // GET BY ID
    [HttpGet("{id:int}",
        Name = nameof(GetCourseById))]
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



    // CREATE
    [HttpPost]
    public async Task<IActionResult> CreateCourse(
        CreateCourseRequest request,
        CancellationToken ct)
    {

        if(await courseService.CodeExistsAsync(
            request.Code, ct))
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
            new { id = result.Id },
            result);
    }




    // UPDATE
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        UpdateCourseRequest request,
        CancellationToken ct)
    {

        var exists =
            await courseService.CodeExistsAsync(
                request.Code,
                ct);


        if(exists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail =
                $"Code '{request.Code}' is already used.",
                Status =
                StatusCodes.Status409Conflict
            });
        }


        var result =
            await courseService.UpdateAsync(
                id,
                request,
                ct);


        return result is null
            ? NotFound()
            : Ok(result);
    }




    // DELETE
    [HttpDelete("{id:int}")]
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