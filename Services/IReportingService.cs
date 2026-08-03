
using TmsApi.DTOs;

namespace TmsApi.Services;

public interface IReportingService
{
    Task<IReadOnlyList<StudentPageDto>> GetStudentsPageAsync(
        int page,
        int pageSize,
        CancellationToken ct);

    Task<IReadOnlyList<CourseEnrollmentSummaryDto>> GetTopCoursesAsync(
        CancellationToken ct);
}
