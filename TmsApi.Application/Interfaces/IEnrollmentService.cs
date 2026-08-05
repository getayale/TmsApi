using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct);
        Task<IReadOnlyList<EnrollmentListDto>> GetAllAsync(
    CancellationToken ct);

    Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct);

    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct);

    Task<EnrollmentResponseDto?> UpdateAsync(
        int courseId,
        int id,
        UpdateEnrollmentRequest request,
        CancellationToken ct);

    Task<bool> DeleteAsync(
        int courseId,
        int id,
        CancellationToken ct);

    Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken ct);

    Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct);

    Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct);


        Task<Enrollment?> GetEntityByIdAsync(
    int enrollmentId,
    CancellationToken ct);

Task SaveChangesAsync(
    CancellationToken ct);
}