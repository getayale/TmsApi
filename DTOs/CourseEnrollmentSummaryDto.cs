namespace TmsApi.DTOs;

public record CourseEnrollmentSummaryDto(
    string CourseTitle,
    int EnrollmentCount
);