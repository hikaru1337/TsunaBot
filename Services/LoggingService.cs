using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TsunaBot.Services
{
    public class LoggingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;

        private string LogDirectory { get; }
        private string LogFile => Path.Combine(LogDirectory, $"{DateTime.Now:dd-MM-yy}.txt");

        public LoggingService(DiscordSocketClient discord, CommandService Commands)
        {
            LogDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            _discord = discord;
            _commands = Commands;

            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
        }

        private async Task OnLogAsync(LogMessage msg)
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
            if (!File.Exists(LogFile))
                File.Create(LogFile).Dispose();

            string logText = $"{DateTime.Now:dd.MM.yy HH:mm:ss} [{msg.Severity}]: {msg.Exception?.ToString() ?? msg.Message}";
            Console.WriteLine(logText);

            await File.AppendAllTextAsync(LogFile, logText + "\n");
        }
    }
}
