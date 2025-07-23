using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Passport;
using Telegram.Bot.Types.ReplyMarkups;

namespace Lesson26.BotMessage
{
    interface IButtonSender
    {
        public Task SendInlineButtonAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }

    class BotInlineButtonMessage(ILogger<BotInlineButtonMessage> logger) : IButtonSender
    {
        public string? Style { get; set; }
        public string? Format { get; set; }
        public string? Background { get; set; }
        public string? Seed { get; set; }
        public string? Color { get; set; }
        public bool IsColorActive { get; set; } = true;

        public async Task SendInlineButtonAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var keyboardEmoji = new InlineKeyboardMarkup(
            [
                [
                    InlineKeyboardButton.WithCallbackData("/adventurer-neutral", "/adventurer-neutral"),
                    InlineKeyboardButton.WithCallbackData("/thumbs", "/thumbs")
                ],
                [
                    InlineKeyboardButton.WithCallbackData("/avataaars-neutral", "/avataaars-neutral"),
                    InlineKeyboardButton.WithCallbackData("/bottts", "/bottts")
                ],
                [
                    InlineKeyboardButton.WithCallbackData("/identicon", "/identicon"),
                    InlineKeyboardButton.WithCallbackData("/personas", "/personas")
                ],
                [
                    InlineKeyboardButton.WithCallbackData("/adventurer", "/adventurer"),
                    InlineKeyboardButton.WithCallbackData("/croodles", "/croodles")
                ],
                [
                    InlineKeyboardButton.WithCallbackData("/bottts-neutral", "/bottts-neutral"),
                    InlineKeyboardButton.WithCallbackData("/rings", "/rings")
                ]
            ]);

            var keyboardFormat = new InlineKeyboardMarkup(
            [
                [
                    InlineKeyboardButton.WithCallbackData("🖼️ PNG", "/png"),
                    InlineKeyboardButton.WithCallbackData("🧾 SVG", "/svg")
                ]
            ]);
            var keyboardBackground = new InlineKeyboardMarkup(
            [
                [
                    InlineKeyboardButton.WithCallbackData("♻️ Transparent", "transparent"),
                    InlineKeyboardButton.WithCallbackData("🎨 Solid", "solid")
                ]
            ]);

            if (update.Type == UpdateType.Message && update.Message?.Text is not null)
            {
                var messageText = update.Message.Text.Trim();

                if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogInformation("Request: -start");
                    await botClient.SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "*Hi👋 Welcome to the bot , If you need instruction select option below :*\n- /help",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                }
                else if (messageText.Equals("/help", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogInformation("Request: -help");
                    await botClient.SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "*Please choose one of styles below 😇:*",
                        replyMarkup: keyboardEmoji,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                }
                else if (IsColorActive == false && Background is "solid")
                {
                    IsColorActive = true;
                    Color = IsValidColor(update.Message.Text);
                    if (!string.IsNullOrEmpty(Color))
                    {
                        logger.LogInformation("Request: chosen color is {color}", Color);
                        await botClient.SendMessage(
                            chatId: update.Message.Chat.Id,
                            text: "Please enter seed  ",
                            cancellationToken: cancellationToken
                            );
                    }
                    else
                    {
                        logger.LogInformation("Invalid color is {color}", Color);

                        await botClient.SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "*❌Invalid color, Write correct color:*",
                        replyMarkup: keyboardEmoji,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                        );
                    }

                }
                else if (!string.IsNullOrEmpty(Style) && !string.IsNullOrEmpty(Format) &&
                            !string.IsNullOrEmpty(Background) && string.IsNullOrEmpty(Seed))
                {
                    Seed = update.Message.Text;
                    logger.LogInformation("Reques: Seed is {seed}", Seed);

                    if (Background == "transparent")
                    {
                        if (Format == "/svg")
                        {
                            var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id ?? 0;
                            var svgUrl = $"https://api.dicebear.com/9.x{Style}/svg?seed={Seed}";

                            using var httpClient = new HttpClient();
                            var svgBytes = await httpClient.GetByteArrayAsync(svgUrl, cancellationToken); // SVG ni byte[] ko'rinishida olish
                            using var stream = new MemoryStream(svgBytes); // MemoryStream ga yozish

                            await botClient.SendDocument(
                                chatId: chatId,
                                document: new InputFileStream(stream, "avatar.svg"),
                                caption: $"""
                                        🎨 *Style*: `{Style}`
                                        📄 *Format*: `{Format}`
                                        🌱 *Seed*: `{Seed}`
                                        """,
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (Format is "/png")
                        {
                            logger.LogInformation("Request: chosen format is {format}", Format);
                            long chatId = update.CallbackQuery?.Message?.Chat?.Id
                                ?? update.Message?.Chat?.Id ?? 0;

                            var url = $"https://api.dicebear.com/9.x{Style}/png?seed={Seed}&size=1024";

                            await botClient.SendPhoto(
                                chatId: chatId,
                                photo: url,
                                caption: $"""
                                    🎨 *Style*: `{Style}`
                                    📄 *Format*: `{Format}`
                                    🎉 *Seed*: `{Seed}` 
                                    """,
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                    }
                    else if (Background is "solid")
                    {
                        if (Format is "/svg")
                        {
                            var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id ?? 0;
                            var svgUrl = $"https://api.dicebear.com/9.x{Style}/svg?seed={Seed}&backgroundColor={Color}";
                            using var httpClient = new HttpClient();
                            var svgBytes = await httpClient.GetByteArrayAsync(svgUrl, cancellationToken);
                            using var stream = new MemoryStream(svgBytes);
                            await botClient.SendDocument(
                                chatId: chatId,
                                document: new InputFileStream(stream, "avatar.svg"),
                                caption: $"""
                                    🎨 *Style*: `{Style}`
                                    📄 *Format*: `{Format}`
                                    🏞️ *Background*: `{Background}`
                                    🌱 *Seed*: `{Seed}`
                                    """,
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (Format is "/png")
                        {
                            var pngUrl = $"https://api.dicebear.com/9.x{Style}/png?seed={Seed}&backgroundColor={Color}&size=1024";
                            await botClient.SendPhoto(
                                chatId: update.Message.Chat.Id,
                                photo: pngUrl,
                                caption: $"""
                                    🎨 *Style*: `{Style}`
                                    📄 *Format*: `{Format}`
                                    🏞️ *Background*: `{Background}`
                                    🎉 *Seed*: `{Seed}` 
                                    """,
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                    }
                    Seed = null;
                    Style = null;
                    Format = null;
                    Background = null;
                    Color = null;
                    logger.LogInformation("Request: 🎉 All task is done");
                    await botClient.SendMessage(
                    chatId: update.Message!.Chat.Id,
                    text: "*🎉If you want reuse it , select button :*\n- /help",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                    );
                }
            }


            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data is not null)
            {
                List<string> formats = ["/adventurer-neutral","/thumbs","/avataaars-neutral",
                    "/bottts","/identicon","/personas","/adventurer","/croodles","/bottts-neutral","/rings"];
                var request = update.CallbackQuery.Data;

                await botClient.EditMessageReplyMarkup(
                    chatId: update.CallbackQuery.Message!.Chat.Id,
                    messageId: update.CallbackQuery.Message.MessageId,
                    replyMarkup: null,
                    cancellationToken: cancellationToken
                );
                await botClient.DeleteMessage(
                chatId: update.CallbackQuery.Message.Chat.Id,
                messageId: update.CallbackQuery.Message.MessageId,
                cancellationToken: cancellationToken
                );


                if (formats.Contains(request))
                {
                    Style = request;
                    logger.LogInformation("Chosen style: {style}", request);

                    await botClient.SendMessage(
                        chatId: update.CallbackQuery.Message!.Chat.Id,
                        text: "*Which format do you choose 🤔:*",
                        replyMarkup: keyboardFormat,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                }
                else if (request is "/png" or "/svg")
                {
                    Format = request;
                    logger.LogInformation("Chosen format: {style}", request);

                    await botClient.SendMessage(
                        chatId: update.CallbackQuery.Message!.Chat.Id,
                        text: "*Which background type do you choose 🤔:*",
                        replyMarkup: keyboardBackground,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                }
                else if (request is "transparent" or "solid")
                {
                    logger.LogInformation("Request: Background is {background}", request);
                    Background = request;
                    if (Background is "solid")
                    {
                        IsColorActive = false;
                        await botClient.SendMessage(
                        chatId: update.CallbackQuery.Message!.Chat.Id,
                        text: "*Please enter color :*\n *e.g: red*",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                        );
                    }
                    else if (Background is "transparent")
                    {

                        await botClient.SendMessage(
                        chatId: update.CallbackQuery.Message!.Chat.Id,
                        text: "*Please enter seed :*\n",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                        );
                    }
                }
            }

            string IsValidColor(string color)
            {
                var lowerColor = color.ToLower();
                var colors = new Dictionary<string, string>
                {
                    { "white", "FFFFFF" },
                    { "black", "000000" },
                    { "red", "FF0000" },
                    { "green", "00FF00" },
                    { "blue", "0000FF" },
                    { "yellow", "FFFF00" },
                    { "cyan", "00FFFF" },
                    { "magenta", "FF00FF" },
                    { "gray", "808080" },
                    { "lightgray", "D3D3D3" },
                    { "darkgray", "A9A9A9" },
                    { "orange", "FFA500" },
                    { "purple", "800080" },
                    { "brown", "A52A2A" },
                    { "pink", "FFC0CB" },
                    { "lime", "32CD32" },
                    { "navy", "000080" },
                    { "teal", "008080" },
                    { "olive", "808000" },
                    { "maroon", "800000" }
                };

                return colors.TryGetValue(lowerColor, out var value) ? value : "";

            }


        }

    }
}

