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
    public class UpdateSubscriber(ILogger<UpdateSubscriber> logger, DataContext context)
    {
        private readonly ILogger<UpdateSubscriber> _logger = logger;
        private readonly DataContext _context = context;

        [Function("UpdateSubscriber")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "UpdateSubscriber/{subscriberId}")] HttpRequest req, int subscriberId)
        {
            try
            {
                string body = await new StreamReader(req.Body).ReadToEndAsync();
                if (!string.IsNullOrEmpty(body))
                {
                    var newsletterEntity = JsonConvert.DeserializeObject<NewsletterEntity>(body);

                    if (newsletterEntity != null)
                    {
                        var existingSubscriber = await _context.Newsletters.FirstOrDefaultAsync(s => s.Id == subscriberId);
                        if (existingSubscriber == null)
                        {
                            _logger.LogInformation("No email was found.");
                            return new NotFoundObjectResult(new { Status = 404, Message = "No email was found." });
                        }

                        existingSubscriber.Email = newsletterEntity!.Email;
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Subscriber was updated.");
                        return new OkObjectResult(new { existingSubscriber, Status = 200, Message = "Subscriber was updated." });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new BadRequestObjectResult(new { Status = 400, Message = "Unable to update subscriber." });
        }
    }
}
