namespace TmsApi.DTOs;

public record StudentPageDto(
    int Id,
    string RegistrationNumber,
    string Name,
    decimal GPA,
    bool IsActive
);