using Microsoft.EntityFrameworkCore;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Student.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class StudentService(TmsDbContext context) : IStudentService
{
    public async Task<PagedResponse<StudentResponseDto>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken)
    {
        var query = context.Students
            .AsNoTracking()
            .Where(s => !s.IsDeleted);

        // Filtering
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(s =>
                s.Name.Contains(search) ||
                s.RegistrationNumber.Contains(search));
        }

        // Sorting
        query = request.OrderBy.ToLower() switch
        {
            "name" => request.Descending
                ? query.OrderByDescending(s => s.Name)
                : query.OrderBy(s => s.Name),

            "registrationnumber" => request.Descending
                ? query.OrderByDescending(s => s.RegistrationNumber)
                : query.OrderBy(s => s.RegistrationNumber),

            "gpa" => request.Descending
                ? query.OrderByDescending(s => s.GPA)
                : query.OrderBy(s => s.GPA),

            "isactive" => request.Descending
                ? query.OrderByDescending(s => s.IsActive)
                : query.OrderBy(s => s.IsActive),

            _ => request.Descending
                ? query.OrderByDescending(s => s.Id)
                : query.OrderBy(s => s.Id)
        };

        // Total records
        var totalCount = await query.CountAsync(
            cancellationToken);

        // Pagination
        var students = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                RegistrationNumber = s.RegistrationNumber,
                Version = s.Version,
                IsActive = s.IsActive,
                IsDeleted = s.IsDeleted,
                GPA = s.GPA
            })
            .ToListAsync(cancellationToken);

     return new PagedResponse<StudentResponseDto>
{
    Items = students,
    TotalCount = totalCount,
    Page = request.Page,
    PageSize = request.PageSize
};
    }


    public async Task<StudentResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await context.Students
            .AsNoTracking()
            .Where(s => s.Id == id && !s.IsDeleted)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                RegistrationNumber = s.RegistrationNumber,
                Version = s.Version,
                IsActive = s.IsActive,
                IsDeleted = s.IsDeleted,
                GPA = s.GPA
            })
            .FirstOrDefaultAsync(cancellationToken);
    }


    public async Task<StudentResponseDto> CreateAsync(
        CreateStudentDto dto,
        CancellationToken cancellationToken)
    {
        var student = new Student
        {
            RegistrationNumber = dto.RegistrationNumber,
            Name = dto.Name,
            GPA = dto.GPA
        };

        context.Students.Add(student);

        await context.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(
            student.Id,
            cancellationToken))!;
    }


    public async Task<StudentResponseDto?> UpdateAsync(
        int id,
        UpdateStudentDto dto,
        CancellationToken cancellationToken)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(
                s => s.Id == id && !s.IsDeleted,
                cancellationToken);

        if (student is null)
            return null;

        student.RegistrationNumber = dto.RegistrationNumber;
        student.Name = dto.Name;
        student.GPA = dto.GPA;

        await context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }


    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(
                s => s.Id == id && !s.IsDeleted,
                cancellationToken);

        if (student is null)
            return false;

        // Soft delete
        student.IsDeleted = true;
        student.IsActive = false;

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}