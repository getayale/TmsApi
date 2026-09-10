namespace TmsApi.Domain.Entities;

public class Course
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Title { get; set; }

    public int MaxCapacity { get; set; }

    // Lead instructor assigned to this course.
    public string? InstructorId { get; set; }

    // One course can have many enrollments.
    public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();
}