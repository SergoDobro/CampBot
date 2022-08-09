using CampBot.Classes;
using CampBot.Handlers;
using Dapper;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace CampBot;
class Program
{

    public static TelegramBotClient _bot;
    private static MessageHandler _messageHandler;
    private static CallBackQueryHandler _callBackQueryHandler;
    public static Storage _storage; 
    private static string GetToken()
    {
        return File.ReadAllLines("token.txt")[0];
    }
    public static async Task Main(string[] args)
    {
        _bot = new TelegramBotClient(GetToken());
        SqlMapper.AddTypeHandler(typeof(List<long>), new JsonTypeHandler());
        SqlMapper.AddTypeHandler(typeof(List<int>), new JsonTypeHandler());
        SqlMapper.AddTypeHandler(typeof(List<string>), new JsonTypeHandler());
        _storage = new Storage("Data Source=database.db", _bot);
        _storage.Init();
        _messageHandler = new MessageHandler(_bot,_storage, GetToken());
        _callBackQueryHandler = new CallBackQueryHandler(_bot, _storage);

        var cts = new CancellationTokenSource();
        _bot.StartReceiving(
            HandleUpdateAsync,
            HandlePollingErrorAsync,
            cancellationToken: cts.Token,
            receiverOptions: new ReceiverOptions()
            {
                ThrowPendingUpdates = true
            }
        );

        var botInfo = _bot.GetMeAsync().Result;
        Console.WriteLine($"Бот {botInfo.Username} запущен");

        await Task.Delay(-1);
    }
    private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    await OnMessage(update.Message!, cancellationToken);
                    break;
                case UpdateType.CallbackQuery:
                    await OnCallBackQuery(update.CallbackQuery!, cancellationToken);
                    break;
                default:
                    break;
            }
        }
        catch 
        {
        }
    }
    private static async Task OnMessage(Message message, CancellationToken cancellationToken) => await _messageHandler.OnMessage(message, cancellationToken);
    private static async Task OnCallBackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken) => await _callBackQueryHandler.OnCallBackQuery(callbackQuery, cancellationToken);
    private static Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Бот сломался:\n" + exception);
        Environment.Exit(1);
        return null;
    }
}