# User Verificator ⛔🤖
A Telegram bot (made by @ilor64) that meant to test if a new group users are real users or bots.

## Main technologies
### language
* C#/.Net 5
### Dependencies
* This bot uses [Telegram bot NuGet](https://github.com/TelegramBots/telegram.bot) which uses [Telegram bot API](https://core.telegram.org/bots) (you can find more information in the links).

## ⚙️Setup
### Pre execution steps
* [Download .NET](https://dotnet.microsoft.com/en-us/download)
* Open terminal and run dotnet --info to ensure installation.
* git clone https://github.com/ilorwork/UserVerificator.git
* Download an IDE of your choice. Probably Visual Studio, or VS Code.

### Execution
* Create a json file in the solution folder with the following tamplate:
```
{
  "botToken": "<Your bot's token>",
  "logChatId": "<Chat id which you want the logs to be sent to>"
}
```

* Execute the program using ```dotnet run``` command.
  
## Contributions
Feel free to fork this project, work on it and then make a pull request agains **main** branch. <br/>
If they enhance the code in any way, they are generally accepted. <br/>
Please, **DO NOT PUSH ANY TOKEN OR API KEY**, Pull requests containing sensitive content will never be accepted.

## Questions or Suggestions
Feel free to create issues [here](https://github.com/ilorwork/UserVerificator/issues) whenever you need.
