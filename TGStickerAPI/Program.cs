using TGStickerAPI;
using TGStickerAPI.Services;
using Telegram.Bot;
using CommonTGBotLib.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(); ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Host.ConfigureAppConfiguration((host, config) =>
{
    config.AddJsonFile("appsettings.json",
                        optional: true,
                        reloadOnChange: true);
});

builder.Services.AddHostedService<BotConfigService>();
builder.Services.AddScoped<ITGStickerService, TGStickerService>();
builder.Services.AddScoped<ICachedService, CachedService>();

var botConfig = builder.Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
builder.Services.AddHttpClient("LineStickerToTG")
       .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botConfig.BotToken, httpClient));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
