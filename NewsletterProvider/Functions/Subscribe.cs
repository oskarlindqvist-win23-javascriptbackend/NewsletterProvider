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
    public class Subscribe(ILogger<Subscribe> logger, DataContext context)
    {
        private readonly ILogger<Subscribe> _logger = logger;
        private readonly DataContext _context = context;

        [Function("Subscribe")]
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
                            _logger.LogInformation("Email is already subscribed.");
                            return new ConflictObjectResult(new { Status = 409, Message = "Email is already subscribed." });
                        }

                        _context.Newsletters.Add(subscribeEntity);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Subscriber is now subscribed.");
                        return new OkObjectResult(new { Status = 200, Message = "Subscriber is now subscribed." });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new BadRequestObjectResult(new { Status = 400, Message = "Unable to subscribe." });
        }
    }
}
