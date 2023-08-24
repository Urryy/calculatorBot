using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotCalculatorDelivery.Service;

namespace TelegramBotCalculatorDelivery
{

    public class TelegramBot
    {
        private readonly IConfiguration _configuration;
        private TelegramBotClient client;
        private readonly ITelegramBotService _telegramBotService;
        public TelegramBot(IConfiguration configuration, ITelegramBotService telegramBotService)
        {
            _configuration= configuration;
            _telegramBotService = telegramBotService;
        }

        public async Task<TelegramBotClient> GetBot()
        {
            try
            {
                if (client != null) return client;

                client = await _telegramBotService.GetClient();
                //client = new TelegramBotClient(_configuration["Token"]);

                client.StartReceiving(Update, Error);
                //var webHook = $"https://silklink-tracking.ru:7000/api/message/update";
                //await client.SetWebhookAsync(webHook);

                return client;
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        private async Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            await arg1.DeleteWebhookAsync();
            Console.WriteLine(arg2.Message);
            return;
        }

        private async Task Update(ITelegramBotClient arg1, Update arg2, CancellationToken arg3)
        {
            await _telegramBotService.Calculate(arg2);
            return;
        }
    }
}
