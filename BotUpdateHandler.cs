using Lesson26.BotMessage;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
class BotUpdateHandler(
    IButtonSender botSendMassage,
    ILogger<BotUpdateHandler> logger) : IUpdateHandler
{
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Something happened wrong - {messaga}", exception.Message);
        await Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.CallbackQuery)
            logger.LogInformation("💌 new message from {username}", update.CallbackQuery!.Message!.Chat.FirstName);
        else
            logger.LogInformation("💌 new message from {username}", update.Message!.Chat.FirstName);

        
        try
        {
            await botSendMassage.SendInlineButtonAsync(botClient, update, cancellationToken);
        }
        catch (Exception)
        {
            await botClient.SendMessage(
                        chatId: update.Message!.Chat.Id,
                        text: "*🛠️ The bot is experiencing a technical error, please try again after a while. :*\n- /help",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
            throw;
        }
    }
}