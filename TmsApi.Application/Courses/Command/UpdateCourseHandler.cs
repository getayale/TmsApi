using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class UpdateCourseHandler(
    ICourseService courseService,
    ICachedCourseService cachedCourseService)
    : IRequestHandler<UpdateCourseCommand, bool>
{

    public async Task<bool> Handle(
        UpdateCourseCommand command,
        CancellationToken ct)
    {

        var updated = await courseService.UpdateAsync(
            command.Id,
            new UpdateCourseRequest
            {
                Code = command.Code,
                Title = command.Title,
                MaxCapacity = command.MaxCapacity
            },
            ct);


        if (updated is null)
        {
            return false;
        }


       
        await cachedCourseService
            .InvalidateCourseCacheAsync(ct);


        return true;
    }
}