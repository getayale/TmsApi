namespace TmsApi.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }


    public int StudentId { get; set; }

    public int CourseId { get; set; }


    public decimal? Grade { get; set; }
public bool IsArchived { get; set; } = false;
    public DateTime EnrolledAt { get; set; }
        = DateTime.UtcNow;



    // Navigation to Student
    public Student Student { get; set; } = null!;


    // Navigation to Course
    public Course Course { get; set; } = null!;
}