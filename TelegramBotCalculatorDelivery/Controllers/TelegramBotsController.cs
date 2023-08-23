using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using TelegramBotCalculatorDelivery.Service;

namespace TelegramBotCalculatorDelivery.Controllers
{
    [ApiController]
    [Route("api/message/update")]
    public class TelegramBotsController : ControllerBase
    {
        private readonly ITelegramBotService _telegramBotService;
        public TelegramBotsController(ITelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody]object update)
        {
            var upd = JsonConvert.DeserializeObject<Update>(update.ToString());

            if (upd?.Message?.Chat == null && upd?.CallbackQuery == null) return Ok();

            await _telegramBotService.Calculate(upd);

            return Ok();
        }
    }
}
