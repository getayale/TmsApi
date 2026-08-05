namespace TmsApi.Application.DTOs;

/// <summary>
/// A lightweight, serializable DTO optimized for caching "Popular Courses".
/// This blueprint avoids circular dependencies and stores aggregated data.
/// </summary>
public record CourseDto(
    Guid Id,
    string Title,
    string Code,
    int MaxCapacity,
    // Calculated field: Stored as a flat number, not a complex join.
    int CurrentEnrollmentCount 
);