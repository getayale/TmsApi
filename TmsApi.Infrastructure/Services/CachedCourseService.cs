using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedCourseService(
    HybridCache cache,
    ICourseService service,
    ILogger<CachedCourseService> logger)
    : ICachedCourseService
{
    // GET ALL
    public async Task<IReadOnlyList<CourseResponseDto>> GetAllCoursesAsync(
        CancellationToken ct)
    {
        var key = CacheKeys.CoursesAll;

        var dbHit = false;

        var courses = await cache.GetOrCreateAsync(
            key,
            service,
            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation(
                    "Cache MISS for {Key} fetching from DB",
                    key);

                var result = await state.GetCoursesAsync(
                    new PagedRequest
                    {
                        Page = 1,
                        PageSize = 100
                    },
                    token);

                return result.Items;
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct);

        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }

        return courses;
    }


    // GET BY CODE
    public async Task<CourseResponseDto?> GetCourseAsync(
        string code,
        CancellationToken ct)
    {
        var key = CacheKeys.Course(code);

        var dbHit = false;

        var course = await cache.GetOrCreateAsync(
            key,
            (service, code),
            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation(
                    "Cache MISS for {Key} fetching from DB",
                    key);

                var result = await state.service.GetCoursesAsync(
                    new PagedRequest
                    {
                        Search = state.code
                    },
                    token);

                return result.Items.FirstOrDefault();
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct);

        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }

        return course;
    }


    // GET BY ID
    public async Task<CourseResponseDto?> GetCourseByIdAsync(
        int id,
        CancellationToken ct)
    {
        var key = CacheKeys.CourseById(id);

        var dbHit = false;

        var course = await cache.GetOrCreateAsync(
            key,
            (service, id),
            async (state, token) =>
            {
                dbHit = true;

                logger.LogInformation(
                    "Cache MISS for {Key} fetching from DB",
                    key);

                var result = await state.service.GetCoursesAsync(
                    new PagedRequest
                    {
                        Search = state.id.ToString()
                    },
                    token);

                return result.Items.FirstOrDefault(
                    c => c.Id == state.id);
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct);

        if (!dbHit)
        {
            logger.LogInformation(
                "Cache HIT for {Key}",
                key);
        }

        return course;
    }


    // CREATE
  public async Task<CourseResponseDto> CreateCourseAsync(
    CreateCourseRequest request,
    CancellationToken ct)
{
    var course = await service.CreateAsync(
        request,
        ct);

    await InvalidateCourseCacheAsync(ct);

    return course;
}


    // UPDATE
   public async Task<bool> UpdateCourseAsync(
    int id,
    UpdateCourseRequest request,
    CancellationToken ct)
{
    var course = await service.UpdateAsync(
        id,
        request,
        ct);

    if (course is null)
    {
        return false;
    }

    await InvalidateCourseCacheAsync(ct);

    return true;
}

   public async Task<bool> DeleteCourseAsync(
    int id,
    CancellationToken ct)
{
    var deleted = await service.DeleteAsync(
        id,
        ct);

    if (!deleted)
    {
        return false;
    }

    await InvalidateCourseCacheAsync(ct);

    return true;
}


    // INVALIDATE CACHE
    public async Task InvalidateCourseCacheAsync(
        CancellationToken ct)
    {
        logger.LogInformation(
            "Invalidating cache tag {Tag}",
            CacheKeys.CoursesTag);

        await cache.RemoveByTagAsync(
            CacheKeys.CoursesTag,
            ct);
    }
}