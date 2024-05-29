using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NewsletterProvider.Functions
{
    public class Unsubscribe(ILogger<Unsubscribe> logger, DataContext context)
    {
        private readonly ILogger<Unsubscribe> _logger = logger;
        private readonly DataContext _context = context;

        [Function("Unsubscribe")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                if (!string.IsNullOrEmpty(body))
                {
                    var subscribeEntity = JsonConvert.DeserializeObject<NewsletterEntity>(body);
                    if (subscribeEntity != null)
                    {
                        var existingSubscriber = await _context.Newsletters.FirstOrDefaultAsync(s => s.Email == subscribeEntity.Email);
                        if (existingSubscriber != null)
                        {
                            _context.Remove(existingSubscriber);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Subscriber is now unsubscribed.");
                            return new OkObjectResult(new { Status = 200, Message = "Subscriber was unsubscribed." });
                        }
                    }

                    _logger.LogInformation("No subscriber was found linked to this email.");
                    return new NotFoundObjectResult(new { Status = 404, Message = "No subscriber was found linked to this email." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new BadRequestObjectResult(new { Status = 400, Message = "Unable to unsubscribe." });
        }
    }
}
