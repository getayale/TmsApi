using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface IReportingService
{
    Task<IReadOnlyList<StudentPageDto>> GetStudentsPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);


    Task<IReadOnlyList<CourseEnrollmentSummaryDto>> GetTopCoursesAsync(
        CancellationToken cancellationToken);


    Task<int> GetActiveStudentsCountAsync(
        CancellationToken cancellationToken);


    Task<IReadOnlyList<CourseAverageGpaDto>> GetAverageGpaPerCourseAsync(
        CancellationToken cancellationToken);


    Task<IReadOnlyList<string>> GetStudentsWithoutEnrollmentsAsync(
        CancellationToken cancellationToken);


    Task<IReadOnlyList<StudentEnrollmentReportDto>> GetStudentEnrollmentReportAsync(
        CancellationToken cancellationToken);
}