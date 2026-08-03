using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(
        TmsDbContext context,
        ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }


    public async Task<IReadOnlyList<Enrollment>> GetAllAsync(
        CancellationToken ct)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .AsNoTracking()
            .ToListAsync(ct);
    }


    public async Task<Enrollment?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, ct);


        if (enrollment is null)
        {
            _logger.LogWarning(
                "Enrollment {EnrollmentId} not found",
                id);
        }

        return enrollment;
    }


    public async Task<Enrollment> CreateAsync(
        Enrollment enrollment,
        CancellationToken ct)
    {
        _context.Enrollments.Add(enrollment);

        await _context.SaveChangesAsync(ct);


        _logger.LogInformation(
            "Created enrollment {EnrollmentId}",
            enrollment.Id);


        return enrollment;
    }


    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken ct)
    {
        var enrollment =
            await _context.Enrollments
            .FirstOrDefaultAsync(e => e.Id == id, ct);


        if (enrollment is null)
        {
            _logger.LogWarning(
                "Delete failed enrollment {EnrollmentId} not found",
                id);

            return false;
        }


        _context.Enrollments.Remove(enrollment);

        await _context.SaveChangesAsync(ct);


        _logger.LogInformation(
            "Deleted enrollment {EnrollmentId}",
            id);


        return true;
    }


    // Exercise 9: Bulk archive
    public async Task<int> ArchiveOldEnrollmentsAsync(
        DateTime cutoff,
        CancellationToken ct)
    {
        var affectedRows =
            await _context.Enrollments
                .Where(e => e.EnrolledAt < cutoff)
                .ExecuteUpdateAsync(
                    update => update
                        .SetProperty(
                            e => e.IsArchived,
                            true),
                    ct);


        _logger.LogInformation(
            "Archived {Count} old enrollments",
            affectedRows);


        return affectedRows;
    }
}