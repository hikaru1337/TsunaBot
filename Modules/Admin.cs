using DarlingNet.Services.LocalService;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using TsunaBot.DataBase;
using TsunaBot.DataBase.Models;
using TsunaBot.Services;
using TsunaBot.Services.GetOrCreate;
using static TsunaBot.DataBase.Models.Roles_Type;

namespace TsunaBot.Modules
{

    [RequireUserPermission(GuildPermission.Administrator)]
    [Summary("Администрирование и настройка бота")]
    public class Admin : ModuleBase<SocketCommandContext>
    {

        [Command("coinset"), Alias("cs")]
        [Summary("Выставить серверную валюту пользователю")]
        public async Task coinset(SocketGuildUser User,ulong Coins)
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($"{User}", User.GetAvatarUrl()).WithThumbnailUrl(User.GetAvatarUrl());
                if (Coins <= BotSettings.CoinsMaxUser)
                {
                    if (User == null)
                        User = Context.User as SocketGuildUser;
                    var UserDataBase = await _db.Users.GetOrCreate(User.Id);
                    UserDataBase.Coins = Coins;
                    _db.Users.Update(UserDataBase);
                    await _db.SaveChangesAsync();
                    emb.WithDescription($"Пользователю выставлено {Coins} звездочек.");
                }
                else
                    emb.WithDescription($"Выставить пользователю больше {BotSettings.CoinsMaxUser} звездочек нельзя.");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Command("coinadd"), Alias("ca")]
        [Summary("Выдать серверную валюту пользователю")]
        public async Task coinadd(SocketGuildUser User, ulong Coins)
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($"{User}", User.GetAvatarUrl()).WithThumbnailUrl(User.GetAvatarUrl());
                if (User == null)
                    User = Context.User as SocketGuildUser;
                var UserDataBase = await _db.Users.GetOrCreate(User.Id);
                if ((UserDataBase.Coins + Coins) <= BotSettings.CoinsMaxUser)
                {
                    UserDataBase.Coins += Coins;
                    _db.Users.Update(UserDataBase);
                    await _db.SaveChangesAsync();
                    emb.WithDescription($"Пользователю выдано {Coins} звездочек.");
                }
                else
                    emb.WithDescription($"Выдать такую сумму нельзя, ведь она будет превышать пользовательский лимит в {BotSettings.CoinsMaxUser} звездочек .");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Command("botstatus"), Alias("bs")]
        [Summary("Выставить статус боту")]
        public async Task botstatus([Remainder]string status = null)
        {
            using (db _db = new())
            {
                await Context.Client.SetGameAsync(status, null, ActivityType.Playing);
                var Settings = _db.Settings.FirstOrDefault();
                Settings.BotStatus = status;
                _db.Settings.Update(Settings);
                await _db.SaveChangesAsync();
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor)
                                            .WithAuthor($"Смена статуса", Context.Client.CurrentUser.GetAvatarUrl())
                                            .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                                            .WithDescription($"Боту выставлен статус: `{status}`");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Command("levelroleadd"), Alias("lra")]
        [Summary("Установить уровневую роль")]
        public async Task levelroleadd(SocketRole role, uint level) => await roleadd(role, level, RoleTypeEnum.Level);

        [Command("levelroledel"), Alias("lrd")]
        [Summary("Удалить уровневую роль")]
        public async Task levelroledel(SocketRole role) => await roledel(role, RoleTypeEnum.Level);

        [Command("reproleadd"), Alias("rra")]
        [Summary("Установить репутационную роль")]
        public async Task reproleadd(SocketRole role, uint level) => await roleadd(role, level, RoleTypeEnum.Reputation);

        [Command("reproledel"), Alias("rrd")]
        [Summary("Удалить репутационную роль")]
        public async Task reproledel(SocketRole role) => await roledel(role, RoleTypeEnum.Reputation);

        [Command("buyroleadd"), Alias("bra")]
        [Summary("Выставить роль на продажу")]
        public async Task buyroleadd(SocketRole role, uint price) => await roleadd(role, price, RoleTypeEnum.Buy);

        [Command("buyroledel"), Alias("brd")]
        [Summary("Снять роль с продажи")]
        public async Task buyroledel(SocketRole role) => await roledel(role, RoleTypeEnum.Buy);

        private async Task roleadd(SocketRole role, uint value, RoleTypeEnum Type)
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($"🔨 Добавить {(Type == RoleTypeEnum.Level ? "уровневую" : Type == RoleTypeEnum.Reputation ? "репутационную" : "магазинную")} роль");
                var lvlrole = _db.Roles_Type.FirstOrDefault(x => x.RoleId == role.Id && x.Type == Type);
                if (lvlrole == null)
                {
                    var rolepos = role.Guild.CurrentUser.Roles.FirstOrDefault(x => x.Position > role.Position);
                    if (rolepos != null)
                    {
                        if (!role.IsManaged)
                        {
                            var Settings = _db.Settings.FirstOrDefault();
                            emb.WithDescription($"Роль {role.Mention} выставлена за {value} {(Type == RoleTypeEnum.Level ? "уровень" : Type == RoleTypeEnum.Reputation ? "репутации" : "звездочек")}").WithFooter($"Посмотреть ваши {(Type == RoleTypeEnum.Level ? "уровневые"  : Type == RoleTypeEnum.Reputation ? "репутационные" : "магазинные")} роли {Settings.Prefix}{(Type == RoleTypeEnum.Level ? "lr" : Type == RoleTypeEnum.Reputation ? "rr" : "br")}");

                            if (!_db.Roles.Any(x => x.Id == role.Id))
                                _db.Roles.Add(new Roles { Id = role.Id });

                            _db.Roles_Type.Add(new Roles_Type() { RoleId = role.Id, Value = value, Type = Type });
                            await _db.SaveChangesAsync();
                        }
                        else
                            emb.WithDescription("Данную роль нельзя выставить.");
                    }
                }
                else emb.WithDescription($"Роль {role.Mention} уже выдается за {lvlrole.Value} {(Type == RoleTypeEnum.Level ? "уровень" : Type == RoleTypeEnum.Reputation ? "репутации" : "звездочек")}");

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }
        private async Task roledel(SocketRole role, RoleTypeEnum Type)
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($"🔨 Удалить {(Type == RoleTypeEnum.Level ? "уровневую" : Type == RoleTypeEnum.Reputation ? "репутационную" : "магазинную")} роль");
                var lvlrole = _db.Roles_Type.FirstOrDefault(x => x.RoleId == role.Id && x.Type == Type);
                emb.WithDescription($"{(Type == RoleTypeEnum.Level ? "Уровневая" : Type == RoleTypeEnum.Reputation ? "репутационная" : "магазинная")} роль {role.Mention} {(lvlrole != null ? "удалена" : $"не является {(Type == RoleTypeEnum.Level ? "уровневой" : Type == RoleTypeEnum.Reputation ? "репутационной" : "магазинной")}")}.");
                if (lvlrole != null)
                {
                    _db.Roles_Type.Remove(lvlrole);
                    await _db.SaveChangesAsync();
                }
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }



        [Command("embedsay"), Alias("es")]
        [Summary("Написать эмбден сообщение.")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task embedsay(SocketTextChannel TextChannel, [Remainder] string JsonText)
        {
            var mes = JsonToEmbed.JsonCheck(JsonText);
            if (mes.Item1 == null)
            {
                mes.Item2 = null;
                mes.Item1.Color = new Discord.Color(255, 0, 94);
                mes.Item1.WithAuthor("Ошибка!");
                mes.Item1.Description = "Неправильная конвертация в Json.\nПрочтите инструкцию! - [Инструкция](https://docs.darlingbot.ru/commands/komandy-adminov/embedsay)";
            }
            await TextChannel.SendMessageAsync(mes.Item2, false, mes.Item1.Build());
        }

        [Command("say"), Alias("s")]
        [Summary("Написать сообщение")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task say(SocketTextChannel TextChannel, [Remainder] string Text)
        {
            await TextChannel.SendMessageAsync(Text);
        }
    }
}
