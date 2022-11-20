namespace LineStickerToTGBotAPI
{
    public class BotConfiguration
    {
        public string BotToken { get; init; } = default!;
        public string BotHostAddress { get; init; } = default!;
        public string LineStaticStickerUrl { get; init; } = default!;
        public string LineAnimatedStickerUrl { get; init; } = default!;
    }
}
