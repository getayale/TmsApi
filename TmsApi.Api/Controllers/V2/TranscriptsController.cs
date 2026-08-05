using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Controllers.V2;


[ApiController]
[Route("api/v{version:apiVersion}/transcripts")]
[ApiVersion("2.0")]
public class TranscriptsController : ControllerBase
{


    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public IActionResult RequestTranscript(
        [FromBody] object request)
    {
        return Ok(new
        {
            message = "Transcript request accepted"
        });
    }

}