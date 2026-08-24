using MediatR;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Student.DTOs;

namespace TmsApi.Application.Student.Command;


public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, bool>
{
    private readonly IStudentService _studentService;

    public DeleteStudentCommandHandler(IStudentService studentService)
    {
        _studentService=studentService;
    }

    public async Task<bool> Handle(DeleteStudentCommand request,CancellationToken cancellationToken)
    {
        return await _studentService.DeleteAsync(request.id,cancellationToken);
    }
}