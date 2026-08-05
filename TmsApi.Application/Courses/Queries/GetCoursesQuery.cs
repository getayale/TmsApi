using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Courses.Queries;

public record GetCoursesQuery(
    int Page = 1,
    int PageSize = 20
)
: IRequest<PagedResponse<CourseResponseDto>>;