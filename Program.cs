using DarlingNet.Services.LocalService;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using TsunaBot.Services;

namespace TsunaBot
{
    sealed class Program
    {
        IConfigurationRoot Configuration { get; }
        Program()
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory);
            builder.AddYamlFile(BotSettings.config_file);
            Configuration = builder.Build();
        }

        static async Task Main() => await new Program().RunAsync();

        public async Task RunAsync()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU"); // Мы русские - с нами бог.
            var services = ConfigureServices();
            services.GetRequiredService<LoggingService>();      // Start the logging service
            services.GetRequiredService<CommandHandler>(); 		// Start the command handler service
            services.GetRequiredService<DiscordSocketClient>();
            await services.GetRequiredService<StartUpService>().StartAsync();       // Start the startup service
            await Task.Delay(-1);
        }

        ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                    .AddSingleton(new DiscordSocketClient(
                    new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        MessageCacheSize = 128,
                        DefaultRetryMode = RetryMode.Retry502,
                        AlwaysDownloadUsers = true,

                        GatewayIntents = GatewayIntents.All
                    }))
                    .AddSingleton(new CommandService(
                    new CommandServiceConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        DefaultRunMode = RunMode.Async,
                    }))
                    .AddSingleton<CommandHandler>()
                    .AddSingleton<LoggingService>()
                    .AddSingleton<StartUpService>()
                    .AddSingleton<VoiceActive>()
                    .AddSingleton(Configuration)
                    //.AddLogging()
                    .BuildServiceProvider();
        }
    }
}
