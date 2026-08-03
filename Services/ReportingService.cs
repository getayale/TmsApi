using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.DTOs;

namespace TmsApi.Services;

public class ReportingService(TmsDbContext context) : IReportingService
{
    public async Task<IReadOnlyList<StudentPageDto>> GetStudentsPageAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        return await context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentPageDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.IsActive))
            .ToListAsync(ct);
    }


    public async Task<IReadOnlyList<CourseEnrollmentSummaryDto>> GetTopCoursesAsync(
        CancellationToken ct)
    {
        return await context.Courses
            .Select(c => new CourseEnrollmentSummaryDto(
                c.Title,
                c.Enrollments.Count))
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync(ct);
    }
}