using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class EnrollmentService(
    TmsDbContext context,
    ILogger<EnrollmentService> logger)
    : IEnrollmentService
{

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
                e.EnrolledAt))
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
                e.EnrolledAt))
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
}