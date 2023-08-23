using Telegram.Bot.Types;

namespace TelegramBotCalculatorDelivery.Service
{
    public interface ITelegramBotService
    {
        Task<bool> Calculate(Update upd);
    }
}
