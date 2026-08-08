using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface ICachedCourseService
{
    Task<IReadOnlyList<CourseResponseDto>> GetAllCoursesAsync(
        CancellationToken ct);

    Task<CourseResponseDto?> GetCourseAsync(
        string code,
        CancellationToken ct);

    Task InvalidateCourseCacheAsync(
        CancellationToken ct);
}