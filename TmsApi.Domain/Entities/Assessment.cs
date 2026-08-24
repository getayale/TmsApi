namespace TmsApi.Domain.Entities;

public class Assessment
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public decimal MaxScore { get; set; }

    
    public decimal Weight { get; set; }

    // Foreign key
    public int CourseId { get; set; }

    // Navigation property
    public Course Course { get; set; } = null!;
}