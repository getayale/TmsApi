using System.Threading.Channels;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/transcripts")]
[ApiVersion("2.0")]
public class TranscriptsController(
    Channel<TranscriptRequest> channel,
    ITranscriptStatusStore statusStore)
    : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscript(
        TranscriptRequest request,
        [FromHeader(Name = "Idempotency-Key")]
        string? idempotencyKey,
        CancellationToken ct)
    {
        // 1. Check whether this request was already submitted
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing =
                await statusStore
                    .GetReportIdForIdempotencyKeyAsync(
                        idempotencyKey,
                        ct);

            if (existing is not null)
            {
                var existingStatus =
                    await statusStore.GetAsync(
                        existing,
                        ct);

                Response.Headers.RetryAfter = "5";

                return Accepted(
                    Url.Action(
                        nameof(GetStatus),
                        new { id = existing }),
                    existingStatus);
            }
        }


        // 2. Create a new report ID
        var reportId =
            Guid.NewGuid()
                .ToString("N")[..12];


        // 3. Create initial Queued status
        var status =
            await statusStore.CreateAsync(
                reportId,
                request.StudentId,
                ct);


        // 4. Remember the idempotency key
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await statusStore.LinkIdempotencyKeyAsync(
                idempotencyKey,
                reportId,
                ct);
        }


        // 5. Put the job into the background queue
        await channel.Writer.WriteAsync(
            request.WithReportId(reportId),
            ct);


        // 6. Tell the client to poll later
        Response.Headers.RetryAfter = "5";

        return Accepted(
            Url.Action(
                nameof(GetStatus),
                new { id = reportId }),
            status);
    }


    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(
        string id,
        CancellationToken ct)
    {
        var status =
            await statusStore.GetAsync(
                id,
                ct);

        if (status is null)
        {
            return NotFound(
                new ProblemDetails
                {
                    Title = "Transcript not found",

                    Detail =
                        $"No transcript request with id '{id}'.",

                    Status =
                        StatusCodes.Status404NotFound
                });
        }

        return Ok(status);
    }
}