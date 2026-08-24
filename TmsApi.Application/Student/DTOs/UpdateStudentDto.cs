namespace TmsApi.Application.Student.DTOs;

public record UpdateStudentDto
{
   

    public required string RegistrationNumber { get; set; }

    public required string Name { get; set; }

    public decimal GPA { get; set; }

   

  
}