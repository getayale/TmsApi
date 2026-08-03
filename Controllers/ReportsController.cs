using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{

    // 1. Count active students with GPA >= 3.0
    [HttpGet("active-students")]
    public async Task<IActionResult> GetActiveStudents()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(new
        {
            ActiveStudents = count
        });
    }


    // 2. Courses with most enrollments
    [HttpGet("course-enrollments")]
    public async Task<IActionResult> GetCourseEnrollmentCount()
    {
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(list);
    }


    // 3. Average GPA per course
    [HttpGet("average-gpa")]
    public async Task<IActionResult> GetAverageGpa()
    {
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(list);
    }


    // 4A. Students with zero enrollments using NOT EXISTS
    [HttpGet("students-without-enrollments")]
    public async Task<IActionResult> GetStudentsWithoutEnrollments()
    {
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }
}