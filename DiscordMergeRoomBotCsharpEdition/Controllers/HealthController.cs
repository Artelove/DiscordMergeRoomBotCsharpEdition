using Microsoft.AspNetCore.Mvc;

namespace DiscordMergeRoomBotCsharpEdition.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public Task Get()
        {
            return Task.FromResult(Ok("Healthy"));
        }
    }
}
