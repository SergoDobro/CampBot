using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types;
using Telegram.Bot;
using CampBot.Classes;

namespace CampBot.Handlers
{
    internal class MessageHandler
    {
        private string botToken;
        private TelegramBotClient _bot;
        private Storage _storage;
        private HttpClient _httpClient = new();
        public MessageHandler(TelegramBotClient _bot, Storage _storage, string botToken)
        {
            this.botToken = botToken;
            this._bot = _bot;
            this._storage = _storage;

        }
        public async Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            Console.WriteLine(message.Chat.Id);
            await _storage.AddNewUser(message.From);
            if (message.Text.StartsWith("/addun"))
            {
                if (message.Text.Split(' ').Count()>1)
                {
                    string data = message.Text.Remove(0,7);
                    if (data.StartsWith('@'))
                        data = data.Remove(0, 1);
                    bool res = await _storage.SetUserMasterByUserName(message.From.Id, data);
                    if (res)
                        await _bot.SendTextMessageAsync(message.From.Id, $"Человек добавлен в список наблюдения");
                    else
                        await _bot.SendTextMessageAsync(message.From.Id, $"Не удалось добавить человека в список");
                    return;
                }
                else
                {
                    await _bot.SendTextMessageAsync(message.From.Id, $"Введите параметры через пробел");
                }
            }
            else if (message.Text.StartsWith("/id"))
            {
                await _bot.SendTextMessageAsync(message.From.Id, $"{message.From.Id}");
                return;
            }
            else if (message.Text.StartsWith("/addid"))
            {
                if (message.Text.Split(' ').Length > 2)
                {
                    await _storage.SetUserMaster(message.From.Id, long.Parse(message.Text.Split(' ')[1]),
                        message.Text.Remove(0,
                        message.Text.Split(' ')[0].Length + message.Text.Split(' ')[1].Length + 2));
                }
                else
                {
                    await _storage.SetUserMaster(message.From.Id, long.Parse(message.Text.Split(' ')[1]), "");
                }
                await _bot.SendTextMessageAsync(message.From.Id, $"{message.Text.Split(' ')[1]}");
                return;
            }
            else
            {
                string text = "Добро пожаловать:";
                await _bot.SendTextMessageAsync(message.From.Id, text,
                    replyMarkup: new Commands.Command_GetText().GetKeyboard());
            }
        }
    }
}
