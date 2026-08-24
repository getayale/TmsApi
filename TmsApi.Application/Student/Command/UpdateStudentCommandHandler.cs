using MediatR;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Student.DTOs;

namespace TmsApi.Application.Student.Command;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, StudentResponseDto>
{
    private readonly IStudentService _studentService;

    public UpdateStudentCommandHandler(IStudentService studentService)
    {
      _studentService=studentService;  
    }

 public async Task<StudentResponseDto?> Handle(UpdateStudentCommand request,CancellationToken cancellationToken)
    {
        return await _studentService.UpdateAsync(request.id,request.Dto,cancellationToken);
    }   
}