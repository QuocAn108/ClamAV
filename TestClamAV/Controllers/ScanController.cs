using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using nClam;
using System.Text;
using TestClamAV.Services;

namespace TestClamAV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScanController : ControllerBase
    {
        private readonly IClamAVService _clamAVService;

        public ScanController(IClamAVService clamAVService)
        {
            _clamAVService = clamAVService;
        }

        [HttpPost("scan-file")]
        public async Task<IActionResult> ScanFile([FromForm] string filePath)
        {
            var result = await _clamAVService.ScanFile(filePath);
            return Ok(new { Result = result });
        }

        [HttpPost("scan-html")]
        public async Task<IActionResult> ScanHtml([FromForm] string htmlContent)
        {
            var result = await _clamAVService.ScanHtml(htmlContent);
            return Ok(new { Result = result });
        }

        [HttpPost("scan-pdf")]
        public async Task<IActionResult> ScanPdfAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var result = await _clamAVService.ScanFileAsync(stream);
            return Ok(new { Result = result });
        }
        [HttpGet("api/scan/ping")]
        public async Task<IActionResult> PingClamAV()
        {
            try
            {
                var clam = new ClamClient("localhost", 3310);
                var ping = await clam.PingAsync();

                if (ping)
                    return Ok("ClamAV is alive.");
                else
                    return StatusCode(500, "ClamAV did not respond to ping.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error pinging ClamAV: {ex.Message}");
            }
        }
    }
}
