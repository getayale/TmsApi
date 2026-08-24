namespace TmsApi.Application.Student.DTOs;

public record CreateStudentDto
{
   

    public required string RegistrationNumber { get; set; }

    public required string Name { get; set; }

    public decimal GPA { get; set; }

   

  
}