using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotCalculatorDelivery.Service
{
    public interface ITelegramBotService
    {
        Task<bool> Calculate(Update upd);
        Task<TelegramBotClient> GetClient();
    }
}
