using MediatR;

using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;

using TmsApi.Domain.Enums;

namespace TmsApi.Application.Enrollments.Commands;

public class ApproveEnrollmentHandler(
    IEnrollmentService enrollmentService)
    : IRequestHandler<
        ApproveEnrollmentCommand,
        Result<bool, EnrollmentError>>
{
    public async Task<Result<bool, EnrollmentError>> Handle(
        ApproveEnrollmentCommand command,
        CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetEntityByIdAsync(
            command.EnrollmentId,
            ct);

        if (enrollment is null)
        {
            return Result<bool, EnrollmentError>.Failure(
                new EnrollmentError(
                    "enrollment_not_found",
                    $"Enrollment {command.EnrollmentId} was not found."));
        }

        if (enrollment.Status == EnrollmentStatus.Approved)
        {
            return Result<bool, EnrollmentError>.Failure(
                new EnrollmentError(
                    "already_approved",
                    "Enrollment has already been approved."));
        }

        enrollment.Status = EnrollmentStatus.Approved;

        await enrollmentService.SaveChangesAsync(ct);

        return Result<bool, EnrollmentError>.Success(true);
    }
}