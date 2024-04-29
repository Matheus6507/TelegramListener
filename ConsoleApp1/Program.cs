using Newtonsoft.Json;
using System.Net.Http.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var botClient = new TelegramBotClient("");

using var cts = new CancellationTokenSource();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { } // receive all update types
};

botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);

Console.WriteLine($"Bot started. Press any key to exit.");
Console.ReadKey();

// Stop the bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type != UpdateType.Message)
        return;

    var message = update.Message!;

    if (message.Text is not null)
    {
        Console.WriteLine($"Received a text message in chat {message.Chat.Id}.");

        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri("https://localhost:7259/api/Assistant");
            var content = new { message = message.Text, phone = "" };
            var response = await httpClient.PostAsJsonAsync("https://localhost:7259/api/Assistant" + "/Telegram", content);

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: await response.Content.ReadAsStringAsync(),
                cancellationToken: cancellationToken);
        }
    }
}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
