using MediatR;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Student.DTOs;

namespace TmsApi.Application.Student.Queries;

public class GetStudentByIdQueryHandler
    : IRequestHandler<GetStudentByIdQuery, StudentResponseDto?>
{
    private readonly IStudentService _studentService;

    public GetStudentByIdQueryHandler(
        IStudentService studentService)
    {
        _studentService = studentService;
    }

    public async Task<StudentResponseDto?> Handle(
        GetStudentByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _studentService.GetByIdAsync(
            request.id,
            cancellationToken);
    }
}