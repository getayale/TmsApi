using MediatR;
using TmsApi.Application.Student.DTOs;
namespace TmsApi.Application.Student.Command;

public record CreateStudentCommand(CreateStudentDto Dto):IRequest<StudentResponseDto>;