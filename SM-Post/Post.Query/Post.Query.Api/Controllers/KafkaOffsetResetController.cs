using CQRS.Core.Consumers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Post.Query.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class KafkaOffsetResetController : ControllerBase
    {
        private readonly IEventConsumer _eventConsumer;

        public KafkaOffsetResetController(IEventConsumer eventConsumer)
        {
            _eventConsumer = eventConsumer;
        }

        [HttpPost]
        public async Task<IActionResult> ResetKafkaOffsetAsync()
        {
            // Environment varijable u launchSettings.json 
            var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
            await _eventConsumer.ResetTopicAsync(topic);

            return Ok();
        }
    }
}
