using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Interfaces;


namespace TmsApi.Api.Controllers.V2;


[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(
    IMediator mediator,
    IEnrollmentService enrollmentService) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct)
    {
        var enrollments = await enrollmentService.GetAllAsync(ct);

        return Ok(enrollments);
    }



    [HttpPost]
    public async Task<IActionResult> Enroll(
        EnrollStudentCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);


        return result.Match<IActionResult>(
            onSuccess: created =>
                CreatedAtAction(
                    nameof(GetSchedule),
                    new
                    {
                        studentId = created.StudentId
                    },
                    created),


            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" =>
                        StatusCodes.Status404NotFound,


                    "course_full" or "already_enrolled" =>
                        StatusCodes.Status409Conflict,


                    _ =>
                        StatusCodes.Status400BadRequest
                };


                return Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}");
            });
    }



    [HttpGet("{studentId}/schedule")]
    public async Task<IActionResult> GetSchedule(
        int studentId,
        CancellationToken ct)
    {
        var schedule = await mediator.Send(
            new GetStudentScheduleQuery(studentId),
            ct);


        return Ok(schedule);
    }


   [HttpPost("{id:int}/approve")]
public async Task<IActionResult> Approve(
    int id,
    CancellationToken ct)
{
    var result = await mediator.Send(
        new ApproveEnrollmentCommand(id),
        ct);


    return result.Match<IActionResult>(

        onSuccess: _ =>
            NoContent(),

        onFailure: error =>
            Problem(
                title: "Approval rejected",
                detail: error.Message,
                statusCode: 400
            )
    );
}
}