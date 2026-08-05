namespace TmsApi.Application.DTOs;
using TmsApi.Domain.Enums;

public record EnrollmentListDto(
    int Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseCode,
    string CourseTitle,
    DateTime EnrolledAt,
    EnrollmentStatus Status
);