using _UI;
using API.Db;
using API.Extensions;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ILogger<SettingsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Settings>? Get()
        {
            _logger.LogInformation("Get message received");

            using (var context = new SettingsContext())
            {
                return context.Settings.ToList();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            _logger.LogInformation("Post message received");

            var headers = Request.Headers;

            foreach (var header in headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            var body = await Request.GetRawBodyAsync().ConfigureAwait(false);

            _logger.LogInformation("Body:");
            _logger.LogInformation(body);

            return Ok();
        }

        [HttpPut]
        public IActionResult Put(ServerOptions server)
        {
            _logger.LogInformation("Put message received");

            using (var context = new SettingsContext())
            {
                var serverOptionsInboundAddress = context.Settings.FirstOrDefault(c => c.Id == "ServerOptions:InboundAddress");
                var serverOptionsInboundPort = context.Settings.FirstOrDefault(c => c.Id == "ServerOptions:InboundPort");

                if (serverOptionsInboundAddress != null)
                {
                    serverOptionsInboundAddress.Value = server.InboundAddress;
                    context.Settings.Update(serverOptionsInboundAddress);
                }

                if (serverOptionsInboundPort != null)
                {
                    serverOptionsInboundPort.Value = server.InboundPort;
                    context.Settings.Update(serverOptionsInboundPort);
                }

                context.SaveChanges();
                return Ok();
            }
        }
    }
}

