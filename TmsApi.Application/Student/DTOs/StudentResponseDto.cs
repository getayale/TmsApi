namespace TmsApi.Application.Student.DTOs;

public record StudentResponseDto
{
     public int Id { get; set; }

    public required string RegistrationNumber { get; set; }

    public required string Name { get; set; }

    public decimal GPA { get; set; }

    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; } 

    // Concurrency token
    public uint Version { get; set; }
    
}