using _UI;
using API.Db;
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
            using (var context = new SettingsContext())
            {
                return context.Settings.ToList();
            }
        }

        [HttpPut]
        public IActionResult Put(ServerOptions server)
        {
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

