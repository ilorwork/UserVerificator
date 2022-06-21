using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient("<Your bot token>");

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    errorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Type != UpdateType.Message)
        return;

    // ChatMembersAdded messages
    if (update.Message!.Type == MessageType.ChatMembersAdded)
    {
        OnMemberAdded(update.Message.From, update.Message.Chat.Id, cancellationToken);
    }

    // Only process text messages
    if (update.Message!.Type != MessageType.Text)
        return;

    // Send a message to the user
    await botClient.SendTextMessageAsync(
        chatId: update.Message.Chat.Id,
        text: $"{update.Message.From.FirstName}, You said: {update.Message.Text}",
        cancellationToken: cancellationToken);
}

async void OnMemberAdded(User user, long chatId, CancellationToken cancellationToken)
{
    Console.WriteLine($"User: '{user.FirstName}' id: {user.Id}, added!!!");

    // Send a test message to the user
    await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: $"Welcome: {user.FirstName}! \n" +
              $"Please solve this: 2+3 \n",
        cancellationToken: cancellationToken) ;
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(errorMessage);
    return Task.CompletedTask;
}