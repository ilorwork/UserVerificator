using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UserVerificator;

var config = Configuration.LoadConfiguration();

// Your bot's token - load from userVerificatorConfig.json
var botClient = new TelegramBotClient(config.botToken);

// Chat id which you want the logs to be sent to(Optional) - load from userVerificatorConfig.json
var logChatId = config.logChatId;

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

Log($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Type != UpdateType.Message)
        return;

    var messageType = update.Message!.Type;
    // In case of user adding another user "user" and "newChatusers[0]" would be different
    var user = update.Message.From;
    var newChatUsers = update.Message.NewChatMembers;
    var userId = user!.Id;
    var userFirstName = user.FirstName;
    var chatId = update.Message.Chat.Id;

    Log($"Message type: {messageType}");

    // ChatMemberLeft messages
    if (messageType == MessageType.ChatMemberLeft)
    {
        // In case the user is leaving before answering
        if (usersUnderTest.ContainsKey(userId))
            usersUnderTest.Remove(userId);
    }

    // ChatMembersAdded messages
    if (messageType == MessageType.ChatMembersAdded)
    {
        var chatAdminsTask = botClient.GetChatAdministratorsAsync(chatId);
        var chatAdmins = chatAdminsTask.Result;

        foreach (var chatAdmin in chatAdmins)
        {
            // In case the user is an admin or user added by admin
            if (chatAdmin.User.Id.Equals(user.Id))
                return;
        }
        foreach (var newChatUser in newChatUsers)
        {
            OnMemberAdded(newChatUser, chatId, cancellationToken);
        }
    }

    // Only process text messages
    if (messageType != MessageType.Text)
        return;

    var messageText = update.Message.Text;

    Log($"Received a message: '{messageText}' id: {update.Message.MessageId} from: {userFirstName} id: {userId} in chat {chatId}.");

    if (usersUnderTest.ContainsKey(userId))
    {
        int messageTextAsInt;
        try
        {
            messageTextAsInt = Convert.ToInt32(messageText);
        }
        catch (Exception ex) when (ex is FormatException || ex is OverflowException)
        {
            messageTextAsInt = -1;
        }

        Log($"Test result: {usersUnderTest[userId]}, The user's answer: {messageTextAsInt}");

        // User sent the correct answer
        if (usersUnderTest[userId] == messageTextAsInt)
        {
            usersUnderTest.Remove(userId);

            // Send a "Well done" message to the user
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Well done {userFirstName}! \n" +
                      "You've passed the verification process!",
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.BanChatMemberAsync(chatId, userId, cancellationToken: cancellationToken);
            usersUnderTest.Remove(userId);

            // TODO: send a message to the user with link to kicked out group
            // Send a "kicked out" message to the group
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"{userFirstName} has being kicked out, \n" +
                      "because he/it sent the wrong answer!",
                cancellationToken: cancellationToken);
        }
    }
}

async void OnMemberAdded(User user, long chatId, CancellationToken cancellationToken)
{
    var userId = user!.Id;
    var userFirstName = user.FirstName;

    // In any case that the user is already in the dictionary (for example when this bot is kicked and added again)
    if (usersUnderTest.ContainsKey(userId))
        usersUnderTest.Remove(userId);

    Log($"User: '{userFirstName}' id: {userId}, added!!!");
    var rand = new Random();
    var a = rand.Next(2, 11);
    var b = rand.Next(2, 21);

    // Send a test message to the user
    await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: $"Welcome: {userFirstName}! \n" +
              $"Please solve this: {a}+{b} \n" +
              "Please note! \n" +
              "If you send the wrong answer you will get kicked out of this group!",
        cancellationToken: cancellationToken);

    var result = a + b;
    usersUnderTest.Add(userId, result);
}

async void Log(string logMessage)
{
    Console.WriteLine(logMessage);

    try
    {
        var chatIdAsInt = Convert.ToInt64(logChatId);
        if (chatIdAsInt == 0)
            return;
    }
    catch (FormatException)
    {
        return;
    }

    // Send a test message to the user
    await botClient.SendTextMessageAsync(
        chatId: logChatId,
        text: logMessage);
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Log(errorMessage);
    return Task.CompletedTask;
}