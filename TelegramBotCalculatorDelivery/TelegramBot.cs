using System.Diagnostics;
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
                if (client != null)
                {
                    await client.ReceiveAsync(Update, Error);
                    return client;
                }

                client = await _telegramBotService.GetClient();
                
                await client.ReceiveAsync(Update, Error);

                //WEBHOOK
                //client = new TelegramBotClient(_configuration["Token"]);
                //var webHook = $"https://silklink-tracking.ru:7000/api/message/update";
                //await client.SetWebhookAsync(webHook);

                return client;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Debug.WriteLine(ex.Message);
                await GetBot();
                return client;
            }
            
        }

        private async Task Error(ITelegramBotClient arg1, Exception exc, CancellationToken cts)
        {
            Console.WriteLine(exc.Message);
            Debug.WriteLine(exc.Message);

            await client.CloseAsync();
            await GetBot();
            return;
        }

        private async Task Update(ITelegramBotClient arg1, Update upd, CancellationToken cts)
        {
            try
            {
                await _telegramBotService.Calculate(upd);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Debug.WriteLine(ex.Message);
                await GetBot();
                return;
            }
            
        }
    }
}
