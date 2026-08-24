namespace TmsApi.Application.Student.DTOs;

public record DeleteStudentDto
{
   

    public required string RegistrationNumber { get; set; }

    public required string Name { get; set; }

    public decimal GPA { get; set; }

   

  
}