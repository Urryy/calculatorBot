using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotCalculatorDelivery.Commands;

namespace TelegramBotCalculatorDelivery.Service
{
    public class TelegramBotService : ITelegramBotService
    {
        private TelegramBotClient _botClient;
        private static BaseCommand _command = new BaseCommand();
        public TelegramBotService()
        {
        }

        public async Task<TelegramBotClient> GetClient()
        {
            if (_botClient != null) return _botClient;

            _botClient = new TelegramBotClient("6567574057:AAHV3VaudTxt-9E9yZDjsAeYFwTWjYxqRYs");
            return _botClient;
        }

        public async Task<bool> Calculate(Update upd)
        {
            var isFinish = false;
            if (upd?.Message?.Chat == null && upd?.CallbackQuery == null)
                return false;

            if (upd.Message != null && upd.Message.Text.Contains("start"))
                await ExecuteStart(upd);

            else if (upd.Message.Chat != null && _command.Name == "WeightCommand")
                await ExecuteWeightOfGood(upd);

            else if (upd.Message.Chat != null && _command.Name == "DensityCommand")
                await ExecuteDensityOfGood(upd);

            if (_command.Name == "FinishCommand")
                isFinish = await ExecutePrice(upd);

            if (isFinish == true)
            {
                _command.Name = "start";
                _command.Density = 0;
                _command.Weight = 0;
            }
            else
            {

            }
            return true;   
        }

        private async Task<bool> ExecuteStart(Update upd)
        {
            await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, "Добро пожаловать! Я бот SilkLink Cargo Калькулятор.\n\nГотов помочь вам сделать примерный расчет стоимости доставки. " +
                                                                        "Пожалуйста, поделитесь информацией, чтобы я мог выполнить расчет для вас.");
            await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, "Введите пожалуйста вес вашего товара в килограммах,\nнапример: 50.0");
            _command.Name = "WeightCommand";
            return true;
        }

        private async Task<bool> ExecuteWeightOfGood(Update upd)
        {
            if(double.TryParse(upd.Message.Text.Replace(".", ","), out double resultWeight))
            {
                _command.Weight = resultWeight;
                await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, "Введите пожалуйста объем вашего товара в метр куб (м³),\nнапример 10.0");
                _command.Name = "DensityCommand";
                return true;
            }
            else
            {
                await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, "Введите пожалуйста корректный вес вашего товара!");
                return false;
            }
        }

        private async Task<bool> ExecuteDensityOfGood(Update upd)
        {
            if (double.TryParse(upd.Message.Text.Replace(".", ","), out double resultDensity))
            {
                _command.Density = resultDensity;
                await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, "Идет примерный расчет доставки в $ вашего товара.");
                _command.Name = "FinishCommand";
                return true;
            }
            else
            {
                await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, "Введите пожалуйста корректный объем вашего товара!");
                return false;
            }
        }

        private async Task<bool> ExecutePrice(Update upd)
        {
            if(_command.Density == 0)
            {
                await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, $"При расчете произошла ошибка");
                _command.Name = "FinishCommand";
                return true;
            }

            if(_command.Weight != 0 && _command.Density != 0)
            {
                var devideWeightDestiny = _command.Weight / _command.Density;

                if(devideWeightDestiny < 100)
                {
                    await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, $"У вас низкая плотность: {devideWeightDestiny.ToString("0.00")}. Груз занимает много места и мало весит.\nОбратитесь к менеджеру.\n\n@silklink_cargo\n\n");
                    _command.Name = "FinishCommand";
                    return true;
                }

                if(_command.Weight > 1000)
                {
                    await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, $"У вас большой вес вашего товара и для вас будет скидка.\nЧтобы узнать подробнее обратитесь к менеджеру.\n\n@silklink_cargo\n\n");
                }

                var valueSuperExpress = await ExecuteSuperExpress(devideWeightDestiny);
                var valueExpress = await ExecuteExpress(devideWeightDestiny);
                var valueStandard = await ExecuteStandard(devideWeightDestiny);

                var stringStandardValue = string.Empty;
                if (valueStandard == -1.0)
                {
                    stringStandardValue = "При расчете стандартной доставки возникла ошибка, попробуйте снова.";
                }
                else
                {
                    stringStandardValue = $"Стандарт Авто(25-35 дней) - {valueStandard.ToString("0.00")}$ | {(valueStandard / _command.Weight).ToString("0.00")}$/кг";
                }

                var stringExpressValue = string.Empty;
                if (valueStandard == -1.0)
                {
                    stringExpressValue = "При расчете экспресс доставки возникла ошибка, попробуйте снова.";
                }
                else
                {
                    stringExpressValue = $"Экспресс Авто(11-16 дней) - {valueExpress.ToString("0.00")}$ | {(valueExpress / _command.Weight).ToString("0.00")}$/кг";
                }

                var stringSuperExpressValue = string.Empty;
                if (valueStandard == -1.0)
                {
                    stringSuperExpressValue = "При расчете супер экспресс доставки возникла ошибка, попробуйте снова.";
                }
                else
                {
                    stringSuperExpressValue = $"Супер Экспресс Авто(7-9 дней) - {valueSuperExpress.ToString("0.00")}$ | {(valueSuperExpress / _command.Weight).ToString("0.00")}$/кг";
                }

                await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, $"Плотность товара: {devideWeightDestiny.ToString("0.00")}\n\n" +
                    $"{stringStandardValue}\n\n{stringExpressValue}\n\n{stringSuperExpressValue}");

                return true;
            }
            else
            {
                return true;
            }
            
        }

        private async Task<double> ExecuteSuperExpress(double value)
        {
            if (_command.Weight >= 100)
            {
                if (value < 100)
                {
                    return _command.Weight * 5.0;
                }
                else if (value >= 100 && value < 110)
                {
                    return _command.Weight * 5.0;
                }
                else if (value >= 110 && value < 120)
                {
                    return _command.Weight * 4.9;
                }
                else if (value >= 120 && value < 130)
                {
                    return _command.Weight * 4.8;
                }
                else if (value >= 130 && value < 140)
                {
                    return _command.Weight * 4.7;
                }
                else if (value >= 140 && value < 150)
                {
                    return _command.Weight * 4.6;
                }
                else if (value >= 150 && value < 160)
                {
                    return _command.Weight * 4.4;
                }
                else if (value >= 160 && value < 170)
                {
                    return _command.Weight * 4.3;
                }
                else if (value >= 170 && value < 180)
                {
                    return _command.Weight * 4.2;
                }
                else if (value >= 180 && value < 190)
                {
                    return _command.Weight * 4.1;
                }
                else if (value >= 190 && value < 200)
                {
                    return _command.Weight * 4.0;
                }
                else if (value >= 200 && value < 250)
                {
                    return _command.Weight * 3.9;
                }
                else if (value >= 250 && value < 300)
                {
                    return _command.Weight * 3.8;
                }
                else if (value >= 300 && value < 350)
                {
                    return _command.Weight * 3.7;
                }
                else if (value >= 350 && value < 400)
                {
                    return _command.Weight * 3.6;
                }
                else if (value >= 400 && value < 600)
                {
                    return _command.Weight * 3.5;
                }
                else if (value >= 600 && value < 800)
                {
                    return _command.Weight * 3.3;
                }
                else if (value >= 800 && value < 1000)
                {
                    return _command.Weight * 3.2;
                }
                else if (value >= 1000 && value < 9000)
                {
                    return _command.Weight * 3.1;
                }
                else
                {
                    return -1.0;
                }
            }
            else if (_command.Weight >= 10 && _command.Weight < 100)
            {
                if (value < 100)
                {
                    return _command.Weight * 5.3;
                }
                else if (value >= 100 && value < 110)
                {
                    return _command.Weight * 5.3;
                }
                else if (value >= 110 && value < 120)
                {
                    return _command.Weight * 5.2;
                }
                else if (value >= 120 && value < 130)
                {
                    return _command.Weight * 5.1;
                }
                else if (value >= 130 && value < 140)
                {
                    return _command.Weight * 4.9;
                }
                else if (value >= 140 && value < 150)
                {
                    return _command.Weight * 4.8;
                }
                else if (value >= 150 && value < 160)
                {
                    return _command.Weight * 4.7;
                }
                else if (value >= 160 && value < 170)
                {
                    return _command.Weight * 4.6;
                }
                else if (value >= 170 && value < 180)
                {
                    return _command.Weight * 4.4;
                }
                else if (value >= 180 && value < 190)
                {
                    return _command.Weight * 4.3;
                }
                else if (value >= 190 && value < 200)
                {
                    return _command.Weight * 4.2;
                }
                else if (value >= 200 && value < 250)
                {
                    return _command.Weight * 4.1;
                }
                else if (value >= 250 && value < 300)
                {
                    return _command.Weight * 4.0;
                }
                else if (value >= 300 && value < 350)
                {
                    return _command.Weight * 3.9;
                }
                else if (value >= 350 && value < 400)
                {
                    return _command.Weight * 3.8;
                }
                else if (value >= 400 && value < 600)
                {
                    return _command.Weight * 3.7;
                }
                else if (value >= 600 && value < 800)
                {
                    return _command.Weight * 3.5;
                }
                else if (value >= 800 && value < 1000)
                {
                    return _command.Weight * 3.4;
                }
                else if (value >= 1000 && value < 9000)
                {
                    return _command.Weight * 3.3;
                }
                else
                {
                    return -1.0;
                }
            }
            else if (_command.Weight >= 0.1 && _command.Weight < 10)
            {
                if (value < 100)
                {
                    return _command.Weight * 5.4;
                }
                else if (value >= 100 && value < 110)
                {
                    return _command.Weight * 5.4;
                }
                else if (value >= 110 && value < 120)
                {
                    return _command.Weight * 5.3;
                }
                else if (value >= 120 && value < 130)
                {
                    return _command.Weight * 5.2;
                }
                else if (value >= 130 && value < 140)
                {
                    return _command.Weight * 5.1;
                }
                else if (value >= 140 && value < 150)
                {
                    return _command.Weight * 5.0;
                }
                else if (value >= 150 && value < 160)
                {
                    return _command.Weight * 4.8;
                }
                else if (value >= 160 && value < 170)
                {
                    return _command.Weight * 4.7;
                }
                else if (value >= 170 && value < 180)
                {
                    return _command.Weight * 4.6;
                }
                else if (value >= 180 && value < 190)
                {
                    return _command.Weight * 4.5;
                }
                else if (value >= 190 && value < 200)
                {
                    return _command.Weight * 4.4;
                }
                else if (value >= 200 && value < 250)
                {
                    return _command.Weight * 4.2;
                }
                else if (value >= 250 && value < 300)
                {
                    return _command.Weight * 4.1;
                }
                else if (value >= 300 && value < 350)
                {
                    return _command.Weight * 4.0;
                }
                else if (value >= 350 && value < 400)
                {
                    return _command.Weight * 3.9;
                }
                else if (value >= 400 && value < 600)
                {
                    return _command.Weight * 3.8;
                }
                else if (value >= 600 && value < 800)
                {
                    return _command.Weight * 3.6;
                }
                else if (value >= 800 && value < 1000)
                {
                    return _command.Weight * 3.5;
                }
                else if (value >= 1000 && value < 9000)
                {
                    return _command.Weight * 3.4;
                }
                else
                {
                    return -1.0;
                }
            }
            else
            {
                return -1.0;
            }
        }

        private async Task<double> ExecuteExpress(double value)
        {
            if (_command.Weight >= 100)
            {
                if (value < 100)
                {
                    return _command.Weight * 4.6;
                }
                else if (value >= 100 && value < 110)
                {
                    return _command.Weight * 4.4;
                }
                else if (value >= 110 && value < 120)
                {
                    return _command.Weight * 4.3;
                }
                else if (value >= 120 && value < 130)
                {
                    return _command.Weight * 4.2;
                }
                else if (value >= 130 && value < 140)
                {
                    return _command.Weight * 4.1;
                }
                else if (value >= 140 && value < 150)
                {
                    return _command.Weight * 4.0;
                }
                else if (value >= 150 && value < 160)
                {
                    return _command.Weight * 3.9;
                }
                else if (value >= 160 && value < 170)
                {
                    return _command.Weight * 3.8;
                }
                else if (value >= 170 && value < 180)
                {
                    return _command.Weight * 3.7;
                }
                else if (value >= 180 && value < 190)
                {
                    return _command.Weight * 3.6;
                }
                else if (value >= 190 && value < 200)
                {
                    return _command.Weight * 3.5;
                }
                else if (value >= 200 && value < 250)
                {
                    return _command.Weight * 3.4;
                }
                else if (value >= 250 && value < 300)
                {
                    return _command.Weight * 3.3;
                }
                else if (value >= 300 && value < 350)
                {
                    return _command.Weight * 3.3;
                }
                else if (value >= 350 && value < 400)
                {
                    return _command.Weight * 3.2;
                }
                else if (value >= 400 && value < 600)
                {
                    return _command.Weight * 3.0;
                }
                else if (value >= 600 && value < 800)
                {
                    return _command.Weight * 2.9;
                }
                else if (value >= 800 && value < 1000)
                {
                    return _command.Weight * 2.8;
                }
                else if (value >= 1000 && value < 9000)
                {
                    return _command.Weight * 2.7;
                }
                else
                {
                    return -1.0;
                }
            }
            else if (_command.Weight >= 10 && _command.Weight < 100)
            {
                if (value < 100)
                {
                    return _command.Weight * 4.8;
                }
                else if (value >= 100 && value < 110)
                {
                    return _command.Weight * 4.8;
                }
                else if (value >= 110 && value < 120)
                {
                    return _command.Weight * 4.6;
                }
                else if (value >= 120 && value < 130)
                {
                    return _command.Weight * 4.5;
                }
                else if (value >= 130 && value < 140)
                {
                    return _command.Weight * 4.4;
                }
                else if (value >= 140 && value < 150)
                {
                    return _command.Weight * 4.3;
                }
                else if (value >= 150 && value < 160)
                {
                    return _command.Weight * 4.2;
                }
                else if (value >= 160 && value < 170)
                {
                    return _command.Weight * 4.1;
                }
                else if (value >= 170 && value < 180)
                {
                    return _command.Weight * 4.0;
                }
                else if (value >= 180 && value < 190)
                {
                    return _command.Weight * 3.8;
                }
                else if (value >= 190 && value < 200)
                {
                    return _command.Weight * 3.7;
                }
                else if (value >= 200 && value < 250)
                {
                    return _command.Weight * 3.6;
                }
                else if (value >= 250 && value < 300)
                {
                    return _command.Weight * 3.5;
                }
                else if (value >= 300 && value < 350)
                {
                    return _command.Weight * 3.4;
                }
                else if (value >= 350 && value < 400)
                {
                    return _command.Weight * 3.3;
                }
                else if (value >= 400 && value < 600)
                {
                    return _command.Weight * 3.1;
                }
                else if (value >= 600 && value < 800)
                {
                    return _command.Weight * 3.0;
                }
                else if (value >= 800 && value < 1000)
                {
                    return _command.Weight * 2.9;
                }
                else if (value >= 1000 && value < 9000)
                {
                    return _command.Weight * 2.8;
                }
                else
                {
                    return -1.0;
                }
            }
            else if (_command.Weight >= 0.1 && _command.Weight < 10)
            {
                if (value < 100)
                {
                    return _command.Weight * 5.0;
                }
                else if (value >= 100 && value < 110)
                {
                    return _command.Weight * 5.0;
                }
                else if (value >= 110 && value < 120)
                {
                    return _command.Weight * 4.8;
                }
                else if (value >= 120 && value < 130)
                {
                    return _command.Weight * 4.7;
                }
                else if (value >= 130 && value < 140)
                {
                    return _command.Weight * 4.6;
                }
                else if (value >= 140 && value < 150)
                {
                    return _command.Weight * 4.5;
                }
                else if (value >= 150 && value < 160)
                {
                    return _command.Weight * 4.4;
                }
                else if (value >= 160 && value < 170)
                {
                    return _command.Weight * 4.2;
                }
                else if (value >= 170 && value < 180)
                {
                    return _command.Weight * 4.1;
                }
                else if (value >= 180 && value < 190)
                {
                    return _command.Weight * 4.0;
                }
                else if (value >= 190 && value < 200)
                {
                    return _command.Weight * 3.9;
                }
                else if (value >= 200 && value < 250)
                {
                    return _command.Weight * 3.8;
                }
                else if (value >= 250 && value < 300)
                {
                    return _command.Weight * 3.6;
                }
                else if (value >= 300 && value < 350)
                {
                    return _command.Weight * 3.5;
                }
                else if (value >= 350 && value < 400)
                {
                    return _command.Weight * 3.4;
                }
                else if (value >= 400 && value < 600)
                {
                    return _command.Weight * 3.3;
                }
                else if (value >= 600 && value < 800)
                {
                    return _command.Weight * 3.2;
                }
                else if (value >= 800 && value < 1000)
                {
                    return _command.Weight * 3.0;
                }
                else if (value >= 1000 && value < 9000)
                {
                    return _command.Weight * 2.9;
                }
                else
                {
                    return -1.0;
                }
            }
            else
            {
                return -1.0;
            }
        }

        private async Task<double> ExecuteStandard(double value)
        {
            if (_command.Weight >= 100)
            {
                if (value < 100)
                {
                    return _command.Weight * 4.1;
                }
                else if (value >= 100 && value < 110)
                {
                    return _command.Weight * 4.1;
                }
                else if (value >= 110 && value < 120)
                {
                    return _command.Weight * 4.0;
                }
                else if (value >= 120 && value < 130)
                {
                    return _command.Weight * 3.9;
                }
                else if (value >= 130 && value < 140)
                {
                    return _command.Weight * 3.8;
                }
                else if (value >= 140 && value < 150)
                {
                    return _command.Weight * 3.7;
                }
                else if (value >= 150 && value < 160)
                {
                    return _command.Weight * 3.6;
                }
                else if (value >= 160 && value < 170)
                {
                    return _command.Weight * 3.5;
                }
                else if (value >= 170 && value < 180)
                {
                    return _command.Weight * 3.4;
                }
                else if (value >= 180 && value < 190)
                {
                    return _command.Weight * 3.2;
                }
                else if (value >= 190 && value < 200)
                {
                    return _command.Weight * 3.1;
                }
                else if (value >= 200 && value < 250)
                {
                    return _command.Weight * 3.0;
                }
                else if (value >= 250 && value < 300)
                {
                    return _command.Weight * 2.9;
                }
                else if (value >= 300 && value < 350)
                {
                    return _command.Weight * 2.8;
                }
                else if (value >= 350 && value < 400)
                {
                    return _command.Weight * 2.7;
                }
                else if (value >= 400 && value < 500)
                {
                    return _command.Weight * 2.6;
                }
                else if (value >= 500 && value < 600)
                {
                    return _command.Weight * 2.5;
                }
                else if (value >= 600 && value < 800)
                {
                    return _command.Weight * 2.3;
                }
                else if (value >= 800 && value < 1000)
                {
                    return _command.Weight * 2.2;
                }
                else if (value >= 1000 && value < 9000)
                {
                    return _command.Weight * 2.1;
                }
                else
                {
                    return -1.0;
                }
            }
            else if (_command.Weight >= 10 && _command.Weight < 100)
            {
                if (value < 100)
                {
                    return _command.Weight * 4.4;
                }
                else if (value >= 100 && value < 110)
                {
                    return _command.Weight * 4.3;
                }
                else if (value >= 110 && value < 120)
                {
                    return _command.Weight * 4.2;
                }
                else if (value >= 120 && value < 130)
                {
                    return _command.Weight * 4.1;
                }
                else if (value >= 130 && value < 140)
                {
                    return _command.Weight * 4.0;
                }
                else if (value >= 140 && value < 150)
                {
                    return _command.Weight * 3.9;
                }
                else if (value >= 150 && value < 160)
                {
                    return _command.Weight * 3.8;
                }
                else if (value >= 160 && value < 170)
                {
                    return _command.Weight * 3.7;
                }
                else if (value >= 170 && value < 180)
                {
                    return _command.Weight * 3.5;
                }
                else if (value >= 180 && value < 190)
                {
                    return _command.Weight * 3.4;
                }
                else if (value >= 190 && value < 200)
                {
                    return _command.Weight * 3.3;
                }
                else if (value >= 200 && value < 250)
                {
                    return _command.Weight * 3.2;
                }
                else if (value >= 250 && value < 300)
                {
                    return _command.Weight * 3.1;
                }
                else if (value >= 300 && value < 350)
                {
                    return _command.Weight * 3.0;
                }
                else if (value >= 350 && value < 400)
                {
                    return _command.Weight * 2.9;
                }
                else if (value >= 400 && value < 500)
                {
                    return _command.Weight * 2.8;
                }
                else if (value >= 500 && value < 600)
                {
                    return _command.Weight * 2.6;
                }
                else if (value >= 600 && value < 800)
                {
                    return _command.Weight * 2.5;
                }
                else if (value >= 800 && value < 1000)
                {
                    return _command.Weight * 2.3;
                }
                else if (value >= 1000 && value < 9000)
                {
                    return _command.Weight * 2.2;
                }
                else
                {
                    return -1.0;
                }
            }
            else if (_command.Weight >= 0.1 && _command.Weight < 10)
            {
                if (value < 100)
                {
                    return _command.Weight * 4.4;
                }
                else if (value >= 100 && value < 110)
                {
                    return _command.Weight * 4.4;
                }
                else if (value >= 110 && value < 120)
                {
                    return _command.Weight * 4.3;
                }
                else if (value >= 120 && value < 130)
                {
                    return _command.Weight * 4.2;
                }
                else if (value >= 130 && value < 140)
                {
                    return _command.Weight * 4.2;
                }
                else if (value >= 140 && value < 150)
                {
                    return _command.Weight * 4.1;
                }
                else if (value >= 150 && value < 160)
                {
                    return _command.Weight * 4.0;
                }
                else if (value >= 160 && value < 170)
                {
                    return _command.Weight * 3.8;
                }
                else if (value >= 170 && value < 180)
                {
                    return _command.Weight * 3.6;
                }
                else if (value >= 180 && value < 190)
                {
                    return _command.Weight * 3.5;
                }
                else if (value >= 190 && value < 200)
                {
                    return _command.Weight * 3.4;
                }
                else if (value >= 200 && value < 250)
                {
                    return _command.Weight * 3.3;
                }
                else if (value >= 250 && value < 300)
                {
                    return _command.Weight * 3.2;
                }
                else if (value >= 300 && value < 350)
                {
                    return _command.Weight * 3.1;
                }
                else if (value >= 350 && value < 400)
                {
                    return _command.Weight * 3.0;
                }
                else if (value >= 400 && value < 500)
                {
                    return _command.Weight * 2.9;
                }
                else if (value >= 500 && value < 600)
                {
                    return _command.Weight * 2.8;
                }
                else if (value >= 600 && value < 800)
                {
                    return _command.Weight * 2.6;
                }
                else if (value >= 800 && value < 1000)
                {
                    return _command.Weight * 2.4;
                }
                else if (value >= 1000 && value < 9000)
                {
                    return _command.Weight * 2.3;
                }
                else
                {
                    return -1.0;
                }
            }
            else
            {
                return -1.0;
            }
            
        }
    }
}
