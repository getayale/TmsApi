using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class GetCourseByIdHandler(
    ICachedCourseService cachedService)
    : IRequestHandler<GetCourseByIdQuery, CourseResponseDto?>
{
    public async Task<CourseResponseDto?> Handle(
        GetCourseByIdQuery request,
        CancellationToken ct)
    {
        var courses =
            await cachedService.GetAllCoursesAsync(ct);

        return courses.FirstOrDefault(
            c => c.Id == request.Id);
    }
}