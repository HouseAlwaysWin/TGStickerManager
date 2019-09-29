using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MartinBot.Net.Config;
using MartinBot.Net.Services;
using MartinBot.Net.Services.interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace MartinBot.Net {
    public class Startup {
        private ITelegramBotClient BotClient;
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_2);
            var config = new GlobalConfig ();

            Configuration.Bind ("GlobalConfig", config);
            BotClient = new TelegramBotClient (config.BotConfig.BotToken);
            BotClient.SetWebhookAsync (config.BotConfig.WebhookUrl).Wait ();

            services.AddSingleton (config);
            services.AddSingleton (BotClient);
            services.AddSingleton<IUpdateService, UpdateService> ();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts ();
            }

            app.UseHttpsRedirection ();
            app.UseMvc ();
        }
    }
}