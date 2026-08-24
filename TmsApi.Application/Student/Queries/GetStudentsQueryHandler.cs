using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Student.DTOs;

namespace TmsApi.Application.Student.Queries;

public class GetStudentsQueryHandler
    : IRequestHandler<
        GetStudentsQuery,
        PagedResponse<StudentResponseDto>>
{
    private readonly IStudentService _studentService;

    public GetStudentsQueryHandler(
        IStudentService studentService)
    {
        _studentService = studentService;
    }

    public async Task<PagedResponse<StudentResponseDto>> Handle(
        GetStudentsQuery request,
        CancellationToken cancellationToken)
    {
        return await _studentService.GetAllAsync(
            request.Request,
            cancellationToken);
    }
}