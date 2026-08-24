using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Student.Command;
using TmsApi.Application.Student.DTOs;
using TmsApi.Application.Student.Queries;

namespace TmsApi.Api.Controllers.v2;

[ApiController]
[Route("api/v{version:apiVersion}/students")]
[ApiVersion("2.0")]
[ApiExplorerSettings(GroupName = "v2")]
public class StudentsController(IMediator mediator) : ControllerBase
{
    // GET: api/v2/students
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetStudentsQuery(request),
            cancellationToken);

        return Ok(result);
    }


    // GET: api/v2/students/5
    [HttpGet("{id:int}", Name = "GetStudentById")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var student = await mediator.Send(
            new GetStudentByIdQuery(id),
            cancellationToken);

        if (student is null)
            return NotFound();

        return Ok(student);
    }


    // POST: api/v2/students
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateStudentDto dto,
        CancellationToken cancellationToken)
    {
        var student = await mediator.Send(
            new CreateStudentCommand(dto),
            cancellationToken);

        return CreatedAtRoute(
            "GetStudentById",
            new { id = student.Id },
            student);
    }


    // PUT: api/v2/students/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateStudentDto dto,
        CancellationToken cancellationToken)
    {
        var student = await mediator.Send(
            new UpdateStudentCommand(id, dto),
            cancellationToken);

        if (student is null)
            return NotFound();

        return Ok(student);
    }


    // DELETE: api/v2/students/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(
            new DeleteStudentCommand(id),
            cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}