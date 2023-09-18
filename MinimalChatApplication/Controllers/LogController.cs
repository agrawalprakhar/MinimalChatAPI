using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinimalChatApplication.Data;

namespace MinimalChatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly MinimalChatContext _context;

        public LogController(MinimalChatContext context)
        {
            _context = context;
        }

        // GET: api/Logs
        [HttpGet("/api/logs")]
        [Authorize]
        public IActionResult GetLogs([FromQuery] DateTime? startTime = null, [FromQuery] DateTime? endTime = null)
        {

            var logsQuery = _context.Log.AsQueryable();

            // Filter logs based on the provided start and end times, if any
            if (startTime != null)
            {
                logsQuery = logsQuery.Where(l => l.Timestamp >= startTime);
            }

            if (endTime != null)
            {
                logsQuery = logsQuery.Where(l => l.Timestamp <= endTime);
            }

            var logs = logsQuery.ToList();

            logs.ForEach(l =>
            {
                string cleanedString = l.RequestBody.Replace("\r\n", "").Replace(" ", "").Replace("\\", "").Replace("\n","");
                Console.WriteLine(cleanedString);
                l.RequestBody = cleanedString;
            });

            // If no logs found based on the provided filter, return 404 Not Found
            if (logs.Count == 0)
            {
                return NotFound(new { error = "No logs found." });
            }

            return Ok(new { Logs = logs });


        }

    }
}
