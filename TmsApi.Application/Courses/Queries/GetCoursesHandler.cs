using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;


namespace TmsApi.Application.Courses.Queries;


public class GetCoursesHandler(
    ICachedCourseService cachedService)
    :
    IRequestHandler<
        GetCoursesQuery,
        PagedResponse<CourseResponseDto>>
{


    public async Task<PagedResponse<CourseResponseDto>> Handle(
        GetCoursesQuery request,
        CancellationToken ct)
    {

        var courses =
            await cachedService.GetAllCoursesAsync(ct);



        return new PagedResponse<CourseResponseDto>
        {
            Items = courses
                .Skip(
                    (request.Page - 1)
                    *
                    request.PageSize)

                .Take(request.PageSize)
                .ToList(),


            Page = request.Page,

            PageSize = request.PageSize,

            TotalCount = courses.Count
        };
    }
}