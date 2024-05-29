using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NewsletterProvider.Functions
{
    public class GetSubscriberById(ILogger<GetSubscriberById> logger, DataContext context)
    {
        private readonly ILogger<GetSubscriberById> _logger = logger;
        private readonly DataContext _context = context;

        [Function("GetSubscriberById")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetSubscriberById/{subscriberId}")] HttpRequest req, int subscriberId)
        {
            try
            {
                var subscriber = await _context.Newsletters.FirstOrDefaultAsync(u => u.Id == subscriberId);
                if (subscriber == null)
                {
                    _logger.LogInformation("No subscriber was found.");
                    return new NotFoundObjectResult(new { Status = 404, Message = "No subscriber was found." });
                }
                return new OkObjectResult(subscriber);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
