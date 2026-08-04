using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace TmsApi.Infrastructure.Services;

public class ReportingService(TmsDbContext context) : IReportingService
{
    // Exercise 3: Pagination
    // Stable sorting + Skip/Take translated to SQL LIMIT/OFFSET
    public async Task<IReadOnlyList<StudentPageDto>> GetStudentsPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await context.Students
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentPageDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.IsActive))
            .ToListAsync(cancellationToken);
    }


    // Exercise 3:
    // Top 5 courses by enrollment count
    public async Task<IReadOnlyList<CourseEnrollmentSummaryDto>> GetTopCoursesAsync(
        CancellationToken cancellationToken)
    {
        return await context.Courses
            .AsNoTracking()
            .Select(c => new CourseEnrollmentSummaryDto(
                c.Title,
                c.Enrollments.Count))
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync(cancellationToken);
    }


    // Registrar query:
    // Count active students with GPA >= 3.0
    public async Task<int> GetActiveStudentsCountAsync(
        CancellationToken cancellationToken)
    {
        return await context.Students
            .AsNoTracking()
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync(cancellationToken);
    }


    // Registrar query:
    // Average GPA per course
    public async Task<IReadOnlyList<CourseAverageGpaDto>> GetAverageGpaPerCourseAsync(
        CancellationToken cancellationToken)
    {
        return await context.Enrollments
            .AsNoTracking()
            .GroupBy(e => e.Course.Title)
            .Select(g => new CourseAverageGpaDto(
                g.Key,
                g.Average(e => e.Student.GPA)))
            .ToListAsync(cancellationToken);
    }


    // Registrar query:
    // Students with zero enrollments
    public async Task<IReadOnlyList<string>> GetStudentsWithoutEnrollmentsAsync(
        CancellationToken cancellationToken)
    {
        return await context.Students
            .AsNoTracking()
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync(cancellationToken);
    }


    // Exercise 7:
    // N+1 query fixed using projection
    public async Task<IReadOnlyList<StudentEnrollmentReportDto>> GetStudentEnrollmentReportAsync(
        CancellationToken cancellationToken)
    {
        return await context.Students
            .AsNoTracking()
            .Select(s => new StudentEnrollmentReportDto(
                s.Name,
                s.Enrollments.Count))
            .ToListAsync(cancellationToken);
    }
}