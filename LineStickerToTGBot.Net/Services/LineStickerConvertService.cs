﻿using Telegram.Bot;
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
using LineStickerToTGBotAPI.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace LineStickerToTGBotAPI.Services
{
    public class LineStickerConvertService : ILineStickerConvertService
    {
        private readonly ILogger<LineStickerConvertService> _logger;
        private ITelegramBotClient _botClient;
        private readonly BotConfiguration _botConfig;
        private string _ffmpegPathRootPath;
        private Dictionary<string, BotCommandInfo> _commands;
        public LineStickerConvertService(
            ILogger<LineStickerConvertService> logger,
            IConfiguration configuration,
            ITelegramBotClient telegramBotClient)
        {
            _logger = logger;
            _botClient = telegramBotClient;
            _botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
            _ffmpegPathRootPath = Path.Combine(AppContext.BaseDirectory, "FFmpeg", "ffmpeg.exe");
            _commands = InitCommandsMapperAsync().Result;
        }

        private async Task<Dictionary<string, BotCommandInfo>> InitCommandsMapperAsync()
        {
            var botInfo = await _botClient.GetMeAsync();
            var botName = $"{botInfo.Username}";
            var botCommandInfos = await GetBotCommandInfosAsync();
            Dictionary<string, BotCommandInfo> commands = new Dictionary<string, BotCommandInfo>();
            foreach (var cmd in botCommandInfos)
            {
                commands.Add($@"/{cmd.Command}", cmd);
                commands.Add($@"/{cmd.Command}@{botName}", cmd);
            }
            return commands;
        }

        private async Task<List<BotCommandInfo>> GetBotCommandInfosAsync()
        {
            return new List<BotCommandInfo>
            {
                new($@"menu","選單",async m => await StartCommandAsync(m))
            };
        }

        public async Task InitCommands()
        {
            var botCommandInfos = await GetBotCommandInfosAsync();
            await _botClient.SetMyCommandsAsync(botCommandInfos);
        }

        private async Task StartCommandAsync(Message message)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                           new[]
                           {
                                new InlineKeyboardButton[]
                                {
                                    InlineKeyboardButton.WithCallbackData("新增Line貼圖","NewLineSticker")
                                },
                                new InlineKeyboardButton[]
                                {
                                }
                           });
            await _botClient.SendTextMessageAsync(chatId: message.From.Id,
                                                  text: "請選擇需要的服務",
                                                   replyMarkup: inlineKeyboard);
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
            if (message.Type == MessageType.Text)
            {
                var text = string.IsNullOrEmpty(message.Text) ? "" : message.Text;
                if (_commands.ContainsKey(text))
                {
                    _commands[text].Action.Invoke(message);
                }
            }
            //var rootPath = @"D:\Projects\ffmpeg\stickerpack@2x\animation@2x";
            //var randomName = Guid.NewGuid().ToString("N");
            ////var arg = @$"-i {rootPath}\538098031@2x.png -vf scale=512:-1 {randomName}.webm";
            //var arg = @$"-i pipe:.png -f webm pipe:1";
            var arg = @$" -i pipe:.png -vf scale=512:-1 -f webm pipe:1";
            //var arg = @$"-i pipe:.png -vf scale=512:512:force_original_aspect_ratio=decrease:flags=lanczos -pix_fmt yuva420p -c:v libvpx-vp9 -cpu-used 5 -minrate 50k -b:v 350k -maxrate 450k -to 00:00:02.900 -an -y -f webm pipe:1";
            //var url = @"D:\Projects\ffmpeg\stickerpack@2x\animation@2x\ouput.webm";
            var botInfo = await _botClient.GetMeAsync();
            var stickerName = $"botLineStickerToTg_by_{botInfo.Username}";
            //var fileBytes = await IOFile.ReadAllBytesAsync(url);
            int firstEmojiUnicode = 0x1F601;
            var emojiString = char.ConvertFromUtf32(firstEmojiUnicode);

            //var stickerName = $"test_by_{botInfo.Username}";
            int stickerNumber = 23303;
            var guidstring = Guid.NewGuid().ToString("N");
            //var stickerName = $"linesticker_{guidstring}_by_{botInfo.Username}";
            //await AddAutoLineStickerToVideoStickerAsync(message.From.Id, "", stickerName, stickerNumber);



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

            var stickerSet = await _botClient.GetStickerSetAsync(
               name: stickerName
           );

            foreach (var s in stickerSet.Stickers)
            {
                await _botClient.DeleteStickerFromSetAsync(s.FileId);
            }


            using (var hc = new HttpClient())
            {
                Stream imgsZip = null;
                string downloadUrl = string.Format(_botConfig.LineAnimatedStickerUrl, 23303);
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
                    foreach (var entry in zipFile.Entries.Where(entry => Regex.IsMatch(entry.FullName, @"animation@2x\/\d+\@2x.png")))
                    {
                        //if (Regex.IsMatch(entry.FullName, @"animation@2x\/\d+\@2x.png"))
                        //{
                        try
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {

                                var result = await Cli.Wrap("ffmpeg")
                                             .WithArguments(arg)
                                             .WithStandardInputPipe(PipeSource.FromStream(entry.Open()))
                                             .WithStandardOutputPipe(PipeTarget.ToStream(ms))
                                             .ExecuteAsync();
                                ms.Seek(0, SeekOrigin.Begin);
                                await _botClient.AddVideoStickerToSetAsync(
                                 userId: message.From.Id,
                                 name: stickerName,
                                 webmSticker: new Telegram.Bot.Types.InputFiles.InputFileStream(ms),
                                 emojis: emojiString);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.ToString());
                        }
                        //}
                    }
                }
            }


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

            stickerSet = await _botClient.GetStickerSetAsync(
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

        private async Task CheckAndSetVideoStickerSetAsync(string stickerName, string title, long userId, string emojiString, ZipArchive zipFile, string ffmpegArg)
        {
            try
            {
                var stickerSet = await _botClient.GetStickerSetAsync(name: stickerName);
            }
            catch (Exception ex)
            {
                await AddNewVideoStickerSetAsync(stickerName, title, userId, emojiString, zipFile, ffmpegArg);
            }
        }

        private async Task AddNewVideoStickerSetAsync(string stickerName, string title, long userId, string emojiString, ZipArchive zipFile, string ffmpegArg)
        {
            using (MemoryStream ms = new MemoryStream())
            {

                var firstSticker = zipFile.Entries.FirstOrDefault(entry => Regex.IsMatch(entry.FullName, @"animation@2x\/\d+\@2x.png"));
                if (firstSticker != null)
                {
                    try
                    {
                        var result = await Cli.Wrap("ffmpeg")
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
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                    }
                }
            }
        }

        private async Task AddAutoLineStickerToVideoStickerAsync(long userId, string title, string stickerName, int stickerNumber)
        {

            using (var hc = new HttpClient())
            {
                int emojiUnicode = 0x1F601;
                var botInfo = await _botClient.GetMeAsync();

                var emojiString = char.ConvertFromUtf32(emojiUnicode);
                //var ffmpegArg = @$"-i pipe:.png -vf scale=512:-1 -f webm pipe:1";
                var ffmpegArg = @$"-i pipe:.png -f webm pipe:1";
                //var ffmpegArg = @$"-i pipe:.png -vf scale=512:512:force_original_aspect_ratio=decrease:flags=lanczos -pix_fmt yuva420p -c:v libvpx-vp9 -cpu-used 5 -minrate 50k -b:v 350k -maxrate 450k -to 00:00:02.900 -an -y -f webm pipe:1";
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
                    await CheckAndSetVideoStickerSetAsync(stickerName, title, userId, emojiString, zipFile, ffmpegArg);

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
