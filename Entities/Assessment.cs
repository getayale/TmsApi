namespace TmsApi.Entities;

public class Assessment
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public decimal MaxScore { get; set; }

    // Share of the final grade (example: 0.30 = 30%)
    public decimal Weight { get; set; }

    // Foreign key
    public int CourseId { get; set; }

    // Navigation property
    public Course Course { get; set; } = null!;
}