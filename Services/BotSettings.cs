using CoreRCON;
using Discord;
using System.Net;

namespace TsunaBot.Services
{
    public class BotSettings
    {
        public static string Version = "0.2 TSUNA.NET";
        public static string config_file = "_config.yml";
        public static string ConnectionString = @"Data Source = TsunaNet.db";
        public static string Prefix = "t.";
        public static ulong CoinsMaxUser = 1000000000;

        public static Color TsunaColor = new Color(253, 168, 222);
        public static ulong hikaruId = 551373471536513024;
        public static ulong KranfId = 633057226361274403;
        public static ulong TsunaId = 308965130262282241;
        public static ulong MinecraftRulesChannel = 1003413257698164826;
        public static ulong MinecraftRulesMessage = 1004389585700274266;
        public static ulong MinecraftRoleServer = 1004118038502715514;
    }
}
