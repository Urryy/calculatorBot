﻿using Telegram.Bot;

namespace TelegramBotCalculatorDelivery
{

    public class TelegramBot
    {
        private readonly IConfiguration _configuration;
        private TelegramBotClient client;
        public TelegramBot(IConfiguration configuration)
        {
            _configuration= configuration;
        }

        public async Task<TelegramBotClient> GetBot()
        {
            try
            {
                Console.WriteLine("asdasdasd");
                return new TelegramBotClient(_configuration["Token"]);
                //if (client != null) return client;

                //client = new TelegramBotClient(_configuration["Token"]);

                //var webHook = $"https://silklink-tracking.ru/api/message/update";
                //await client.SetWebhookAsync(webHook);

                //return client;
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
    }
}
