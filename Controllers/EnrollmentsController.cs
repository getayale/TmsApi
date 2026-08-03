using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService)
    : ControllerBase
{


    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(
        typeof(IReadOnlyList<EnrollmentResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("List course enrollments")]
    [EndpointDescription(
        "Returns all enrollments for a specific course.")]
    public async Task<IActionResult> GetAll(
        int courseId,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course is null)
        {
            return NotFound();
        }

        var enrollments =
            await enrollmentService.GetByCourseAsync(courseId, ct);

        return Ok(enrollments);
    }





    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(
        typeof(EnrollmentResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get enrollment by ID")]
    [EndpointDescription(
        "Returns a single enrollment belonging to the specified course.")]
    public async Task<IActionResult> GetEnrollment(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var enrollment =
            await enrollmentService.GetByIdAsync(courseId, id, ct);

        return enrollment is not null
            ? Ok(enrollment)
            : NotFound();
    }





    [HttpPost]
    [ProducesResponseType(
        typeof(EnrollmentResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Enroll a student")]
    [EndpointDescription(
        "Creates a new enrollment for a course. Returns 409 when the course is full.")]
    public async Task<IActionResult> Create(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        var course =
            await courseService.GetByIdAsync(courseId, ct);

        if (course is null)
        {
            return NotFound();
        }


        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail =
                    $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }


        var enrollment =
            await enrollmentService.CreateAsync(
                courseId,
                request,
                ct);


        return CreatedAtAction(
            nameof(GetEnrollment),
            new
            {
                courseId,
                id = enrollment.Id
            },
            enrollment);
    }





    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(EnrollmentResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Update enrollment")]
    [EndpointDescription(
        "Updates the student assigned to an enrollment.")]
    public async Task<IActionResult> Update(
        int courseId,
        int id,
        UpdateEnrollmentRequest request,
        CancellationToken ct)
    {
        var updated =
            await enrollmentService.UpdateAsync(
                courseId,
                id,
                request,
                ct);


        return updated is null
            ? NotFound()
            : Ok(updated);
    }





    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete enrollment")]
    [EndpointDescription(
        "Deletes an enrollment from a course.")]
    public async Task<IActionResult> Delete(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var course =
            await courseService.GetByIdAsync(courseId, ct);


        if (course is null)
        {
            return NotFound();
        }


        var deleted =
            await enrollmentService.DeleteAsync(
                courseId,
                id,
                ct);


        return deleted
            ? NoContent()
            : NotFound();
    }
}