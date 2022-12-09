using TGStickerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Telegram.Bot.Types;

namespace TGStickerAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class BotController : ControllerBase
    {
        private readonly ITGStickerService _lineService;

        public BotController(ITGStickerService lineService)
        {
            _lineService = lineService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleRequest([FromBody] Update update)
        {
            await _lineService.HandleRequestAsync(update);
            return Ok();
        }
    }
}
