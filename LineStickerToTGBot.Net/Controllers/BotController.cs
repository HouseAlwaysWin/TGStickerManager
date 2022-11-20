using LineStickerToTGBotAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Telegram.Bot.Types;

namespace LineStickerToTGBotAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class BotController : ControllerBase
    {
        private readonly ILineStickerConvertService _lineService;

        public BotController(ILineStickerConvertService lineService)
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
