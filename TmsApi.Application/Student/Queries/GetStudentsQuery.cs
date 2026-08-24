using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Student.DTOs;

namespace TmsApi.Application.Student.Queries;

public record GetStudentsQuery(
    PagedRequest Request
) : IRequest<PagedResponse<StudentResponseDto>>;