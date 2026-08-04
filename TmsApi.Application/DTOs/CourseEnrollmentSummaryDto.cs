namespace TmsApi.Application.DTOs;

public record CourseEnrollmentSummaryDto(
    string CourseTitle,
    int EnrollmentCount
);