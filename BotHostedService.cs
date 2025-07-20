using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Lesson26
{
    public class BotHostedService(
        ILogger<BotHostedService> logger,
        ITelegramBotClient botClient,
        IUpdateHandler updateHandler) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var me = await botClient.GetMe(cancellationToken);
            logger.LogInformation("{bot} started successfully", me.FirstName ?? me.Username ?? me.Id.ToString());

            botClient.StartReceiving(
                updateHandler,
                new ReceiverOptions
                {
                    DropPendingUpdates = true,
                    AllowedUpdates= null
                },
                cancellationToken:cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("{service} is exiting", nameof(BotHostedService));
            await Task.CompletedTask;
        }
    }
}
