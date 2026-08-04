namespace TmsApi.Infrastructure.Persistence.Services;

public record EnrollmentRecord(
    string Id,
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt);