using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Courses.Queries;

public record GetCourseByIdQuery(int Id)
    : IRequest<CourseResponseDto?>;