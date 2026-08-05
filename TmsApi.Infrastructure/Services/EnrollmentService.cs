using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace TmsApi.Infrastructure.Services;

public class EnrollmentService(
    TmsDbContext context,
    ILogger<EnrollmentService> logger)
    : IEnrollmentService
{

    public async Task<IReadOnlyList<EnrollmentListDto>> GetAllAsync(
    CancellationToken ct)
{
    return await context.Enrollments
        .AsNoTracking()
        .Include(e => e.Student)
        .Include(e => e.Course)
        .Select(e => new EnrollmentListDto(
            e.Id,
            e.StudentId,
            e.Student.Name,
            e.CourseId,
            e.Course.Code,
            e.Course.Title,
            e.EnrolledAt,
            e.Status
        ))
        .ToListAsync(ct);
}

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt,
                e.Status))
            .ToListAsync(ct);
    }



    public async Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e =>
                e.CourseId == courseId &&
                e.Id == id)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt,
                e.Status))
            .FirstOrDefaultAsync(ct);
    }



    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };


        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);


        logger.LogInformation(
            "Created enrollment {EnrollmentId} for Course {CourseId} and Student {StudentId}",
            enrollment.Id,
            courseId,
            request.StudentId);


        return (await GetByIdAsync(
            courseId,
            enrollment.Id,
            ct))!;
    }



    public async Task<EnrollmentResponseDto?> UpdateAsync(
        int courseId,
        int id,
        UpdateEnrollmentRequest request,
        CancellationToken ct)
    {
        var enrollment = await context.Enrollments
            .FirstOrDefaultAsync(
                e => e.CourseId == courseId &&
                     e.Id == id,
                ct);


        if (enrollment is null)
        {
            return null;
        }


        enrollment.StudentId = request.StudentId;


        await context.SaveChangesAsync(ct);


        logger.LogInformation(
            "Updated enrollment {EnrollmentId}",
            enrollment.Id);


        return await GetByIdAsync(
            courseId,
            id,
            ct);
    }



    public async Task<bool> DeleteAsync(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var enrollment = await context.Enrollments
            .FirstOrDefaultAsync(
                e => e.CourseId == courseId &&
                     e.Id == id,
                ct);


        if (enrollment is null)
        {
            return false;
        }


        context.Enrollments.Remove(enrollment);

        await context.SaveChangesAsync(ct);


        logger.LogInformation(
            "Deleted enrollment {EnrollmentId}",
            enrollment.Id);


        return true;
    }
    public async Task<bool> ExistsAsync(
    int studentId,
    string courseCode,
    CancellationToken ct)
{
    return await context.Enrollments
        .AnyAsync(e =>
            e.StudentId == studentId &&
            e.Course.Code == courseCode,
            ct);
}
public async Task AddAsync(
    Enrollment enrollment,
    CancellationToken ct)
{
    await context.Enrollments.AddAsync(
        enrollment,
        ct);

    await context.SaveChangesAsync(ct);
}
public async Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(
    int studentId,
    CancellationToken ct)
{
    return await context.Enrollments
        .Include(e => e.Course)
        .Where(e => e.StudentId == studentId)
        .AsNoTracking()
        .ToListAsync(ct);
}

public async Task<Enrollment?> GetEntityByIdAsync(
    int enrollmentId,
    CancellationToken ct)
{
    return await context.Enrollments
        .FirstOrDefaultAsync(
            e => e.Id == enrollmentId,
            ct);
}

public async Task SaveChangesAsync(
    CancellationToken ct)
{
    await context.SaveChangesAsync(ct);
}
}