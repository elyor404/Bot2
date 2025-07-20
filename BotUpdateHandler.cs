using Lesson26.BotMessage;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
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
        try
        {
            await botSendMassage.SendInlineButtonAsync(botClient, update, cancellationToken);
        }
        catch (Exception)
        {
            await botClient.SendMessage(
                        chatId: update.Message!.Chat.Id,
                        text: "*🛠️ The bot is experiencing a technical error, please try again after a while. :*\n- /help",
                        cancellationToken: cancellationToken
                    );
            throw;
        }
    }
}