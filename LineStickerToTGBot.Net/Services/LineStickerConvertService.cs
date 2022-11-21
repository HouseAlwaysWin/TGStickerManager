using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using IOFile = System.IO.File;
using System.Diagnostics;
using System.Reflection;
using System.IO.Compression;
using System.Text.RegularExpressions;
using CliWrap;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace LineStickerToTGBotAPI.Services
{
    public class LineStickerConvertService : ILineStickerConvertService
    {
        private readonly ILogger<LineStickerConvertService> _logger;
        private ITelegramBotClient _botClient;
        private readonly BotConfiguration _botConfig;
        private string _ffmpegPathRootPath;
        public LineStickerConvertService(
            ILogger<LineStickerConvertService> logger,
            IConfiguration configuration,
            ITelegramBotClient telegramBotClient)
        {
            _logger = logger;
            _botClient = telegramBotClient;
            _botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
            _ffmpegPathRootPath = Path.Combine(AppContext.BaseDirectory, "FFmpeg");
        }

        public async Task HandleRequestAsync(Update update)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => OnMessageAsync(update.Message!),
                UpdateType.CallbackQuery => OnCallbackQueryAsync(update.CallbackQuery!),
                UpdateType.InlineQuery => OnInlineQueryAsync(update.InlineQuery!),
                _ => UnknownUpdateHandlerAsync(update)

            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {

            }

        }
        private async Task OnMessageAsync(Message message)
        {
            //var rootPath = @"D:\Projects\ffmpeg\stickerpack@2x\animation@2x";
            //var randomName = Guid.NewGuid().ToString("N");
            ////var arg = @$"-i {rootPath}\538098031@2x.png -vf scale=512:-1 {randomName}.webm";
            //var arg = @$"-i pipe:.png -vf  webm pipe:1";
            var arg = @$"-i pipe:.png -vf scale=512:-1 -f webm pipe:1";
            //var url = @"D:\Projects\ffmpeg\stickerpack@2x\animation@2x\ouput.webm";
            var botInfo = await _botClient.GetMeAsync();
            var stickerName = $"botLineStickerToTg_by_{botInfo.Username}";
            //var fileBytes = await IOFile.ReadAllBytesAsync(url);
            //var emojiString = char.ConvertFromUtf32(firstEmojiUnicode);



            //var process = new Process
            //{
            //    StartInfo =
            //    {
            //        FileName = "ffmpeg.exe",
            //        Arguments = arg,
            //        UseShellExecute = false,
            //        CreateNoWindow = true,
            //        RedirectStandardInput = true,
            //    }
            //};

            //process.Start();
            //using (var hc = new HttpClient())
            //{
            //    Stream imgsZip = null;
            //    string downloadUrl = string.Format(_botConfig.LineAnimatedStickerUrl, 23303);
            //    try
            //    {
            //        imgsZip = await hc.GetStreamAsync(downloadUrl);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.Log(LogLevel.Error, $"{ex}");
            //        return;
            //    }
            //    using (ZipArchive zipFile = new ZipArchive(imgsZip))
            //    {
            //        foreach (var entry in zipFile.Entries)
            //        {
            //            if (Regex.IsMatch(entry.FullName, @"animation@2x\/\d+\@2x.png"))
            //            {
            //                try
            //                {
            //                    using (MemoryStream ms = new MemoryStream())
            //                    {

            //                        var result = await Cli.Wrap(_ffmpegPathRootPath)
            //                                     .WithArguments(arg)
            //                                     .WithStandardInputPipe(PipeSource.FromStream(entry.Open()))
            //                                     .WithStandardOutputPipe(PipeTarget.ToStream(ms))
            //                                     .ExecuteAsync();
            //                        ms.Seek(0, SeekOrigin.Begin);
            //                        await _botClient.AddVideoStickerToSetAsync(
            //                         userId: message.From.Id,
            //                         name: stickerName,
            //                         webmSticker: new Telegram.Bot.Types.InputFiles.InputFileStream(ms),
            //                         emojis: emojiString);
            //                    }
            //                }
            //                catch (Exception ex)
            //                {
            //                    _logger.LogError(ex.ToString());
            //                }
            //            }
            //        }
            //    }
            //}


            //try
            //{
            //    using (MemoryStream ms = new MemoryStream())
            //    {
            //        //using (var ffmpegIn = process.StandardOutput.BaseStream)
            //        //{
            //        //var bytes = new byte[65536];
            //        //var result = await ffmpegIn.ReadAsync(bytes);
            //        //ms.Write(bytes);
            //        ms.Seek(0, SeekOrigin.Begin);
            //        await _botClient.AddVideoStickerToSetAsync(
            //            userId: message.From.Id,
            //            name: stickerName,
            //            webmSticker: new Telegram.Bot.Types.InputFiles.InputFileStream(ms),
            //            emojis: emojiString);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.ToString());
            //}

            //process.WaitForExit();



            //try
            //{
            //    using (MemoryStream ms = new MemoryStream())
            //    {
            //        ms.Write(fileBytes);
            //        ms.Seek(0, SeekOrigin.Begin);
            //        await _botClient.CreateNewVideoStickerSetAsync(
            //                            userId: message.From.Id,
            //                            name: stickerName,
            //                            title: "Line貼圖轉換",
            //                            webmSticker: new Telegram.Bot.Types.InputFiles.InputFileStream(ms),
            //                            emojis: emojiString);

            //    }
            //}
            //catch (Exception ex)
            //{

            //    _logger.LogError(ex.ToString());
            //}

            await AddConvertedVideoStickerToTGSticker(
                userId: message.From.Id,
                title: "",
                stickerName: stickerName,
                emojiUnicode: firstEmojiUnicode,);

            var stickerSet = await _botClient.GetStickerSetAsync(
                 name: stickerName
             );

            var sticker = stickerSet.Stickers[0];

            await _botClient.SendStickerAsync(
                chatId: message.From.Id,
                sticker: new Telegram.Bot.Types.InputFiles.InputOnlineFile(sticker.FileId)
                        );

        }

        private async Task<List<Telegram.Bot.Types.File>> UploadedStaticStickerToTGSticker(long userId, int emojiUnicode, int stickerNumber)
        {
            using (var hc = new HttpClient())
            {
                var emojiString = char.ConvertFromUtf32(emojiUnicode);
                Stream imgsZip = null;
                string downloadUrl = string.Format(_botConfig.LineStaticStickerUrl, stickerNumber);
                List<Telegram.Bot.Types.File> files = new List<Telegram.Bot.Types.File>();
                try
                {
                    imgsZip = await hc.GetStreamAsync(downloadUrl);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"{ex}");
                    return files;
                }
                using (ZipArchive zipFile = new ZipArchive(imgsZip))
                {
                    foreach (var entry in zipFile.Entries)
                    {
                        if (Regex.Match(entry.Name, @"^\d+@2x.png").Success)
                        {
                            using (Image image = Image.Load(entry.Open()))
                            using (MemoryStream ms = new MemoryStream())
                            {
                                image.Mutate(m => m.Resize(512, 512));
                                image.SaveAsPng(ms);
                                ms.Seek(0, SeekOrigin.Begin);
                                try
                                {
                                    var file = await _botClient.UploadStickerFileAsync(
                                        userId: userId,
                                        pngSticker: new Telegram.Bot.Types.InputFiles.InputFileStream(ms)
                                    );
                                    files.Add(file);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Log(LogLevel.Error, $"{ex}");
                                    return files;
                                }
                            }
                        }
                    };
                }
                return files;
            }
        }

        private async Task CheckAndSetStickerSetAsync(string stickerName, string title, long userId, string emojiString, ZipArchive zipFile, string ffmpegArg)
        {
            var stickerSet = await _botClient.GetStickerSetAsync(name: stickerName);
            if (stickerSet == null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    var firstSticker = zipFile.Entries.FirstOrDefault(entry => Regex.IsMatch(entry.FullName, @"animation@2x\/\d+\@2x.png"));
                    if (firstSticker != null)
                    {
                        var result = await Cli.Wrap(_ffmpegPathRootPath)
                                     .WithArguments(ffmpegArg)
                                     .WithStandardInputPipe(PipeSource.FromStream(firstSticker.Open()))
                                     .WithStandardOutputPipe(PipeTarget.ToStream(ms))
                                     .ExecuteAsync();
                        ms.Seek(0, SeekOrigin.Begin);
                        await _botClient.CreateNewVideoStickerSetAsync(
                                            userId: userId,
                                            name: stickerName,
                                            title: title,
                                            webmSticker: new Telegram.Bot.Types.InputFiles.InputFileStream(ms),
                                            emojis: emojiString);
                    }
                }
            }
        }

        private async Task AddConvertedVideoStickerToTGSticker(long userId, string title, string stickerName, int stickerNumber)
        {

            using (var hc = new HttpClient())
            {
                int firstEmojiUnicode = 0x1F601;
                var firstEmojiString = char.ConvertFromUtf32(firstEmojiUnicode);
                var ffmpegArg = @$"-i pipe:.png -vf scale=512:-1 -f webm pipe:1";
                Stream imgsZip = null;
                string downloadUrl = string.Format(_botConfig.LineAnimatedStickerUrl, stickerNumber);
                try
                {
                    imgsZip = await hc.GetStreamAsync(downloadUrl);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"{ex}");
                    return;
                }
                using (ZipArchive zipFile = new ZipArchive(imgsZip))
                {
                    try
                    {
                        await CheckAndSetStickerSetAsync(stickerName, title, userId, firstEmojiString, zipFile, ffmpegArg);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                        await CheckAndSetStickerSetAsync(stickerName, title, userId, firstEmojiString, zipFile, ffmpegArg);
                    }

                    foreach (var entry in zipFile.Entries)
                    {
                        if (Regex.IsMatch(entry.FullName, @"animation@2x\/\d+\@2x.png"))
                        {
                            try
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {

                                    var result = await Cli.Wrap(_ffmpegPathRootPath)
                                                 .WithArguments(ffmpegArg)
                                                 .WithStandardInputPipe(PipeSource.FromStream(entry.Open()))
                                                 .WithStandardOutputPipe(PipeTarget.ToStream(ms))
                                                 .ExecuteAsync();
                                    ms.Seek(0, SeekOrigin.Begin);
                                    firstEmojiUnicode++;
                                    var emojiString = char.ConvertFromUtf32(firstEmojiUnicode);

                                    await _botClient.AddVideoStickerToSetAsync(
                                         userId: userId,
                                         name: stickerName,
                                         webmSticker: new Telegram.Bot.Types.InputFiles.InputFileStream(ms),
                                         emojis: emojiString);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex.ToString());
                            }
                        }
                    }
                }
            }
        }

        private async Task OnCallbackQueryAsync(CallbackQuery query)
        {

        }

        private async Task OnInlineQueryAsync(InlineQuery query)
        {

        }

        public Task HandleErrorAsync(Exception exception)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
            return Task.CompletedTask;
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }
    }
}
