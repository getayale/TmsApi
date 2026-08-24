using MediatR;
using TmsApi.Application.Student.DTOs;

namespace TmsApi.Application.Student.Command;

public record UpdateStudentCommand(
    int id,
    UpdateStudentDto Dto
) : IRequest<StudentResponseDto?>;