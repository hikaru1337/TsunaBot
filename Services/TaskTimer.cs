using Discord.WebSocket;
using System.Threading.Tasks;
using System.Timers;
using TsunaBot.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TsunaBot.Services;
using TsunaBot.DataBase.Models;
using DarlingNet.Services.LocalService.VerifiedAction;
using System;


namespace DarlingNet.Services.LocalService
{
    public class TaskTimer
    {
        private readonly DiscordSocketClient _discord;

        public TaskTimer(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public async Task CheckMinecraftSubscribe()
        {
            using (var _db = new db())
            {
                await Task.Delay(5000);

                foreach (var MinecraftUser in _db.Minecraft_User.Include(x=>x.Minecraft_User_Subscribe).Where(x=>x.Minecraft_User_Subscribe.Count > 0))
                {
                    var user = _discord.Guilds.FirstOrDefault().GetUser(MinecraftUser.UsersId);
                    await CheckMinecraftSubscribeNow(MinecraftUser, user);
                }
            }
        }

        async static void RoleCheck(SocketGuildUser user) => await user.RemoveRole(BotSettings.MinecraftRoleServer);

        public static Task CheckMinecraftSubscribeNow(Minecraft_User minecraft_User, SocketGuildUser user)
        {
            var ActiveSubs = minecraft_User.Minecraft_User_Subscribe.FirstOrDefault(x => x.ActiveSubscribe);
            if (ActiveSubs != null && ActiveSubs.SubscribeTo != DateTime.MaxValue)
            {
                var time = (ActiveSubs.SubscribeTo - DateTime.Now).TotalMilliseconds + 5000;
                if (int.MaxValue >= time)
                {
                    Timer TaskTime = new(time);
                    TaskTime.Elapsed += (s, e) => MinecraftSubscribe(minecraft_User.Id);
                    TaskTime.Start();

                    if (ActiveSubs.Premium)
                    {
                        Timer TaskTime2 = new(time);
                        TaskTime.Elapsed += (s, e) => RoleCheck(user);
                        TaskTime.Start();
                    }
                }
            }
            else
                RoleCheck(user);

            return Task.CompletedTask;
        }

        private static async void MinecraftSubscribe(ulong MinecraftAccountId,string username = null)
        {
            using (var _db = new db())
            {
                var MinecraftUser = _db.Minecraft_User.Include(x => x.Minecraft_User_Subscribe).FirstOrDefault(x=> MinecraftAccountId == 0 ? x.UserName == username : x.Id == MinecraftAccountId);
                var ActiveSubs = MinecraftUser.Minecraft_User_Subscribe.FirstOrDefault(x => x.ActiveSubscribe);
                if(ActiveSubs == null)
                    await myrcon.SendQuery($"kick {MinecraftUser.UserName} Подписка на сервер закончилась!", $"whitelist remove {MinecraftUser.UserName}");
            }
        }
    }
}
