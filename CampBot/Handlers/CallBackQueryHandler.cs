using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using CampBot.Commands;
using CampBot.Classes;

namespace CampBot.Handlers
{
    internal class CallBackQueryHandler
    {
        private TelegramBotClient _bot;
        public CallBackQueryHandler(TelegramBotClient _bot, Storage storage)
        {
            this._bot = _bot;
            var g = new CampBot.Commands.Command_GetText();
            commands.Add(g.GetName, g);
            var gg = new CampBot.Commands.ChildPathSelection();
            gg.SetStorage(storage);
            commands.Add(gg.GetName, gg);
            var ggg = new CampBot.Commands.GetChildren();
            ggg.SetStorage(storage);
            commands.Add(ggg.GetName, ggg);
        }
        public async Task OnCallBackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var data = callbackQuery.Data.Split(':');

            commands[data[0]].Execute(_bot, callbackQuery, cancellationToken);
        }
        Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();
    }
}
