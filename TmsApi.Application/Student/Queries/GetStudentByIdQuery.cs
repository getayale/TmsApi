using MediatR;
using TmsApi.Application.Student.DTOs;

namespace TmsApi.Application.Student.Queries;

public record GetStudentByIdQuery(int id):IRequest<StudentResponseDto?>;