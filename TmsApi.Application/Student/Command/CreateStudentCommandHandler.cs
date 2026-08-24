using MediatR;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Student.DTOs;

namespace TmsApi.Application.Student.Command;

public class CreateStudentCommandHandler:IRequestHandler<CreateStudentCommand,StudentResponseDto>
{

    private readonly IStudentService _studentService;


public CreateStudentCommandHandler(IStudentService studentService)
    {
        _studentService=studentService;
    }


 public async Task<StudentResponseDto> Handle(CreateStudentCommand request,CancellationToken cancellationToken)
    {

        return await _studentService.CreateAsync(request.Dto,cancellationToken);
        
    }
     
}