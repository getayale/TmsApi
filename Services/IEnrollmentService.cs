using TmsApi.Entities;

namespace TmsApi.Services;

public interface IEnrollmentService
{
    Task<IReadOnlyList<Enrollment>> GetAllAsync(
        CancellationToken ct);

    Task<Enrollment?> GetByIdAsync(
        int id,
        CancellationToken ct);

    Task<Enrollment> CreateAsync(
        Enrollment enrollment,
        CancellationToken ct);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken ct);

    Task<int> ArchiveOldEnrollmentsAsync(
        DateTime cutoff,
        CancellationToken ct);
}