
using Lesson26;
using Lesson26.BotMessage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddSingleton<ITelegramBotClient, TelegramBotClient>( x =>
    new TelegramBotClient(builder.Configuration["Bot:Token"]
         ?? throw new ArgumentException("Telegram bot token is not configured")));

builder.Services.AddHostedService<BotHostedService>();
builder.Services.AddSingleton<IUpdateHandler, BotUpdateHandler>();
builder.Services.AddSingleton<IButtonSender, BotInlineButtonMessage>();



var app = builder.Build();

app.Run();