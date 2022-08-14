using CoreRCON;
using DarlingNet.Services.LocalService;
using DarlingNet.Services.LocalService.Attribute;
using DarlingNet.Services.LocalService.VerifiedAction;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TsunaBot.DataBase;
using TsunaBot.DataBase.Models;
using TsunaBot.Services;

namespace TsunaBot.Modules
{
    [MinecraftPermission]
    [Summary("Minecraft панель")]
    public class Minecraft : ModuleBase<SocketCommandContext>
    {
        [Command("whitelistadd"), Alias("wla")]
        [Summary("Добавить пользователя в WhiteList")]
        public async Task whitelistadd(SocketGuildUser User,string username, bool Premium, int month = 1)
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($" - Выдать доступ к Minecraft серверу", User.GetAvatarUrl());
                var DiscordUser = _db.Users.Include(x => x.Minecraft_User).ThenInclude(x=>x.Minecraft_User_Subscribe).FirstOrDefault(x=>x.Id == User.Id);

                if(DiscordUser.Minecraft_User != null)
                {
                    var prefix = _db.Settings.FirstOrDefault().Prefix;
                    emb.WithDescription($"Пользователь уже есть в WhiteList. Если вы хотите выдать безлимит или премиум доступ, используйте {prefix}Subscribeadd");
                }
                else
                {
                    var TimeNow = DateTime.Now;
                    DiscordUser.Minecraft_User = new Minecraft_User { RulesAccept = true, UserName = username, Users = DiscordUser };

                    var newsubscribe = new Minecraft_User_Subscribe { SubscribeAt = TimeNow, Minecraft_User = DiscordUser.Minecraft_User, Premium = Premium, SubscribeGivenAdminId = Context.User.Id };

                    if (month == 666)
                        newsubscribe.SubscribeTo = DateTime.MaxValue;
                    else
                    {
                        newsubscribe.SubscribeTo = TimeNow.AddMonths(month);
                        await TaskTimer.CheckMinecraftSubscribeNow(DiscordUser.Minecraft_User, User);
                    }

                    if(Premium)
                        await User.AddRole(BotSettings.MinecraftRoleServer);

                    DiscordUser.Minecraft_User.Minecraft_User_Subscribe.Add(newsubscribe);
                    _db.Users.Update(DiscordUser);
                    await _db.SaveChangesAsync();

                    emb.WithDescription("Выдан доступ:\n" +
                                       $"{User.Mention} - {username}\n" +
                                       $"Premium Доступ: {(Premium ? "Да" : "Нет")}\n" +
                                       $"Доступ: {(month == 666 ? "безлимит" : $"На {month} месяц")}");

                    await myrcon.SendQuery($"whitelist add {username}");
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        public async Task WLPRVT(SocketUser user, string username, string reason)
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($" - Удалить доступ к Minecraft серверу", Context.User.GetAvatarUrl());
                var DiscordUser = _db.Minecraft_User.Include(x => x.Minecraft_User_Subscribe).FirstOrDefault(x => user != null ? x.UsersId == user.Id : username == x.UserName);

                if (DiscordUser != null)
                {
                    var SubscribeList = DiscordUser.Minecraft_User_Subscribe.AsEnumerable();

                    var List = SubscribeList.Where(x => x.SubscribeFuture);
                    var ListDostup = SubscribeList.Where(x => x.ActiveSubscribe || x.SubscribeFuture);

                    if (List.Any())
                        _db.Minecraft_User_Subscribe.RemoveRange(List);

                    emb.WithDescription("Доступ удален:\n" +
                                    $"<@{DiscordUser.UsersId}> - {DiscordUser.UserName}\n" +
                                    $"Premium доступ: {ListDostup.Count(x => x.Premium)} месяца\n" +
                                    $"Обычный доступ: {ListDostup.Count(x => !x.Premium)} месяца\n" +
                                    $"Подписка до: {SubscribeList.Max(x => x.SubscribeTo).ToString("dd.MM.yy HH:mm")}");

                    var ThisSubscribe = SubscribeList.FirstOrDefault(x => x.ActiveSubscribe);
                    if (ThisSubscribe != null)
                    {
                        ThisSubscribe.SubscribeTo = DateTime.Now.AddSeconds(-10);
                        _db.Minecraft_User_Subscribe.Update(ThisSubscribe);
                    }
                    await _db.SaveChangesAsync();
                    await myrcon.SendQuery($"kick {DiscordUser.UserName} Подписка на сервер закончилась!", $"whitelist remove {DiscordUser.UserName}");
                }
                else
                    emb.WithDescription($"Пользователя с таким {(user != null ? "дискордом" : "ником")}, нету в системе.");

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Command("whitelistrem"), Alias("wlr")]
        [Summary("удалить пользователя из WhiteList")]
        public async Task whitelistrem(SocketUser user,string reason) => await WLPRVT(user, null, reason);

        [Command("whitelistrem"), Alias("wlr")]
        [Summary("удалить пользователя из WhiteList")]
        public async Task whitelistrem(string username, string reason) => await WLPRVT(null, username, reason);



        private async Task subscribePRVT(SocketGuildUser User = null, string username = null, int month = 1, bool premium = false)
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($" - Продлить доступ к Minecraft серверу", Context.User.GetAvatarUrl());
                var DiscordUser = _db.Minecraft_User.Include(x => x.Minecraft_User_Subscribe).FirstOrDefault(x => User != null ? x.UsersId == User.Id : username == x.UserName);

                if (DiscordUser != null)
                {
                    DateTime LastTime = DateTime.Now;
                    var newsubscribe = new Minecraft_User_Subscribe { SubscribeAt = LastTime, Premium = premium, Minecraft_User = DiscordUser,SubscribeGivenAdminId = Context.User.Id };
                    if (DiscordUser.Minecraft_User_Subscribe.Count > 0)
                        LastTime = DiscordUser.Minecraft_User_Subscribe.Max(x => x.SubscribeTo);

                    if (month != 666)
                        LastTime = LastTime.AddMonths(month);
                    else
                        LastTime = DateTime.MaxValue;

                    newsubscribe.SubscribeTo = LastTime;

                    if (premium)
                        await (User == null ? Context.Guild.GetUser(DiscordUser.Id) : User).AddRole(BotSettings.MinecraftRoleServer);


                    _db.Minecraft_User_Subscribe.Add(newsubscribe);
                    await _db.SaveChangesAsync();

                    emb.WithDescription($"Доступ продлен на {month} месяц:\n" +
                                    $"<@{DiscordUser.UsersId}> - {DiscordUser.UserName}\n" +
                                    $"Premium Доступ: {(premium ? "Да" : "Нет")}\n" +
                                    $"Подписка до: {LastTime.ToString("dd.MM.yy HH:mm")}");

                    await myrcon.SendQuery($"whitelist add {DiscordUser.UserName}");
                    await TaskTimer.CheckMinecraftSubscribeNow(DiscordUser,User == null ? Context.Guild.GetUser(DiscordUser.Id) : User);
                }
                else
                    emb.WithDescription($"Пользователя с таким {(User != null ? "дискордом" : "ником")}, нету в системе.");

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }

        }

        [Command("subscribeadd"), Alias("sa")]
        [Summary("Продлить подписку")]
        public async Task subscribeAdd(SocketGuildUser User, int month = 1, bool premium = false) => await subscribePRVT(User, null, month, premium);

        [Command("subscribeadd"), Alias("sa")]
        [Summary("Продлить подписку")]
        public async Task subscribeAdd(string username, int month = 1, bool premium = false) => await subscribePRVT(null, username, month, premium);
    }
}
