using Telegram.Bot.Types.Enums;
using Telegram.Bot;

namespace LineStickerToTGBotAPI.Services
{
    public class BotConfigService : IHostedService
    {
        private readonly ILogger<BotConfigService> _logger;
        private readonly IServiceProvider _services;
        private readonly BotConfiguration _botConfig;
        public BotConfigService(ILogger<BotConfigService> logger,
                                IServiceProvider serviceProvider,
                                IConfiguration configuration)
        {
            _logger = logger;
            _services = serviceProvider;
            _botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            #region Setting Webhook
            using var scope = _services.CreateScope();

            var _botService = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            // Configure custom endpoint per Telegram API recommendations:
            // https://core.telegram.org/bots/api#setwebhook
            // If you'd like to make sure that the Webhook request comes from Telegram, we recommend
            // using a secret path in the URL, e.g. https://www.example.com/<token>.
            // Since nobody else knows your bot's token, you can be pretty sure it's us.
            // var webhookAddress = @$"{_botConfig.UnoTGBotHostAddress}/bot/{_botConfig.UnoTGBotToken}";
            var webhookAddress = @$"{_botConfig.BotHostAddress}";
            _logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);
            await _botService.SetWebhookAsync(
                url: webhookAddress,
                allowedUpdates: Array.Empty<UpdateType>(),
                cancellationToken: cancellationToken);
            #endregion

            //await _botService.InitCommands();

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            // Remove webhook upon app shutdown
            _logger.LogInformation("Removing webhook");
            await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        }
    }
}
