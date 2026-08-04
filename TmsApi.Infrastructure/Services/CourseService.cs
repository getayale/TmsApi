using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using Microsoft.Extensions.Logging;


namespace TmsApi.Infrastructure.Services;

public class CourseService(
    TmsDbContext context,
    ILogger<CourseService> logger) : ICourseService
{

   
    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        IQueryable<Course> query =
            context.Courses.AsNoTracking();


        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(
                    c.Title,
                    $"%{request.Search}%")
                ||
                EF.Functions.ILike(
                    c.Code,
                    $"%{request.Search}%"));
        }


        // Count before paging
        var totalCount =
            await query.CountAsync(ct);



        // Sorting
        query = request.OrderBy switch
        {
            "Code" =>
                request.Descending
                ? query.OrderByDescending(c => c.Code)
                : query.OrderBy(c => c.Code),


            "MaxCapacity" =>
                request.Descending
                ? query.OrderByDescending(c => c.MaxCapacity)
                : query.OrderBy(c => c.MaxCapacity),


            _ =>
                request.Descending
                ? query.OrderByDescending(c => c.Title)
                : query.OrderBy(c => c.Title)
        };



        var items =
            await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CourseResponseDto(
                    c.Id,
                    c.Code,
                    c.Title,
                    c.MaxCapacity,
                    c.Enrollments.Count))
                .ToListAsync(ct);



        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }





    // GET BY ID
    public async Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);
    }






    // CREATE
    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct)
    {

        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };


        context.Courses.Add(course);


        await context.SaveChangesAsync(ct);


        logger.LogInformation(
            "Created course {CourseId} {Code}",
            course.Id,
            course.Code);



        return (await GetByIdAsync(course.Id, ct))!;
    }






    // UPDATE
    public async Task<CourseResponseDto?> UpdateAsync(
        int id,
        UpdateCourseRequest request,
        CancellationToken ct)
    {

        var course =
            await context.Courses
                .FirstOrDefaultAsync(
                    c => c.Id == id,
                    ct);


        if(course is null)
            return null;



        course.Code = request.Code;
        course.Title = request.Title;
        course.MaxCapacity = request.MaxCapacity;



        await context.SaveChangesAsync(ct);



        logger.LogInformation(
            "Updated course {CourseId}",
            id);



        return await GetByIdAsync(id, ct);
    }







    // DELETE
    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken ct)
    {

        var course =
            await context.Courses
                .FirstOrDefaultAsync(
                    c => c.Id == id,
                    ct);



        if(course is null)
            return false;



        context.Courses.Remove(course);


        await context.SaveChangesAsync(ct);



        logger.LogInformation(
            "Deleted course {CourseId}",
            id);



        return true;
    }








  
    public async Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking()
            .AnyAsync(
                c => c.Code == code,
                ct);
    }

}