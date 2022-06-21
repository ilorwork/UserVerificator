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
var usersUnderTest = new Dictionary<long, int>();

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

    var user = update.Message.From;
    var userId = user!.Id;
    var chatId = update.Message.Chat.Id;

    // ChatMembersAdded messages
    if (update.Message!.Type == MessageType.ChatMembersAdded)
    {
        OnMemberAdded(update.Message.From, update.Message.Chat.Id, cancellationToken);
    }

    // Only process text messages
    if (update.Message!.Type != MessageType.Text)
        return;

    if (usersUnderTest.ContainsKey(userId))
    {
        // User sent the correct answer
        if (usersUnderTest[userId] == Convert.ToInt32(update.Message.Text))
        {
            usersUnderTest.Remove(userId);

            // Send a test message to the user
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Correct answer!",
                cancellationToken: cancellationToken);
        }
        else
        {
            usersUnderTest.Remove(userId);

            // Send a test message to the user
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Wrong answer!",
                cancellationToken: cancellationToken);

            // TODO: send a message to the user with link to kicked out group
        }
    }
}

async void OnMemberAdded(User user, long chatId, CancellationToken cancellationToken)
{
    Console.WriteLine($"User: '{user.FirstName}' id: {user.Id}, added!!!");
    var rand = new Random();
    var a = rand.Next(2, 11);
    var b = rand.Next(2, 21);

    // Send a test message to the user
    await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: $"Welcome: {user.FirstName}! \n" +
              $"Please solve this: {a}+{b} \n" +
              "Please note! \n" +
              "If you send the wrong answer you will get kicked out of this group!",
        cancellationToken: cancellationToken);

    var result = a + b;
    usersUnderTest.Add(user.Id, result);
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