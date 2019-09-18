using System.Threading.Tasks;
using MartinBot.Net.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace MartinBot.Net.Controllers {
    [Route ("api/[controller]")]
    public class UpdateController : Controller {
        private readonly IUpdateService _updateService;

        public UpdateController (IUpdateService updateService) {
            _updateService = updateService;
        }

        [HttpPost]
        public async Task<IActionResult> Post ([FromBody] Update update) {
            await _updateService.EchoAsync (update);

            return Ok ();
        }

    }
}