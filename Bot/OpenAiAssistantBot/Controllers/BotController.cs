using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace ChatGPTBot.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/messages")]
    [ApiController]
    public class BotController(IBotFrameworkHttpAdapter adapter, IBot bot) : ControllerBase
    {
        [HttpPost]
        [HttpGet]
        public async Task PostAsync()
        {
            await adapter.ProcessAsync(Request, Response, bot);
        }
    }
}
