using MediatR;

namespace TmsApi.Application.Courses.Commands;

public record DeleteCourseCommand(
    int CourseId
) : IRequest<bool>;