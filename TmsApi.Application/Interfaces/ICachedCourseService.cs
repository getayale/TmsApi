using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface ICachedCourseService
{
    // Get all courses
    Task<IReadOnlyList<CourseResponseDto>> GetAllCoursesAsync(
        CancellationToken ct);

    // Get one course by code
    Task<CourseResponseDto?> GetCourseAsync(
        string code,
        CancellationToken ct);

    // Get one course by ID
    Task<CourseResponseDto?> GetCourseByIdAsync(
        int id,
        CancellationToken ct);

    // Create course
    Task<CourseResponseDto> CreateCourseAsync(
        CreateCourseRequest request,
        CancellationToken ct);

    // Update course
    Task<bool> UpdateCourseAsync(
        int id,
        UpdateCourseRequest request,
        CancellationToken ct);

    // Delete course
    Task<bool> DeleteCourseAsync(
        int id,
        CancellationToken ct);

    // Clear/invalidate course cache
    Task InvalidateCourseCacheAsync(
        CancellationToken ct);
}