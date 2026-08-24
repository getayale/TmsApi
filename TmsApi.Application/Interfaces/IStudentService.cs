using TmsApi.Application.DTOs;
using TmsApi.Application.Student.DTOs;

namespace TmsApi.Application.Interfaces;

public interface IStudentService
{
    Task<PagedResponse<StudentResponseDto>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken);

    Task<StudentResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<StudentResponseDto> CreateAsync(
        CreateStudentDto dto,
        CancellationToken cancellationToken);

    Task<StudentResponseDto?> UpdateAsync(
        int id,
        UpdateStudentDto dto,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken);
}