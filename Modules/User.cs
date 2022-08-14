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
using TsunaBot.Services.GetOrCreate;
using static TsunaBot.DataBase.Models.Roles_Type;
using static TsunaBot.Services.CommandHandler;

namespace TsunaBot.Modules
{
    [Summary("Пользовательские команды")]
    public class User : ModuleBase<SocketCommandContext>
    {
        [Command("userinfo"), Alias("ui")]
        [Summary("Получить информацию о пользователе")]
        public async Task userinfo(SocketGuildUser User = null)
        {
            using (db _db = new())
            {
                if (User == null)
                    User = Context.User as SocketGuildUser;
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($"{User}", User.GetAvatarUrl()).WithThumbnailUrl(User.GetAvatarUrl());
                var UserDataBase = _db.Users.Include(x => x.Minecraft_User).ThenInclude(x => x.Minecraft_User_Subscribe).FirstOrDefault(x => x.Id == User.Id);
                uint count = Convert.ToUInt32(UserDataBase.Level * 80 * UserDataBase.Level);
                uint countNext = Convert.ToUInt32((UserDataBase.Level + 1) * 80 * (UserDataBase.Level + 1));

                string DailyCoin = null;
                TimeSpan TimeToDaily = UserDataBase.DailyCoin - DateTime.Now;
                if (TimeToDaily.TotalSeconds < -18000)
                    DailyCoin = "Получение звездочек - доступно сейчас!";
                else
                {
                    if (TimeToDaily.TotalSeconds > 0)
                        DailyCoin = $"До получения звездочек - {TimeToDaily.Hours}:{TimeToDaily.Minutes}:{TimeToDaily.Seconds}";
                    else
                    {
                        DailyCoin = $"До сброса комбо получения звездочек - {TimeToDaily.Hours + 5}:{TimeToDaily.Minutes + 60}:{TimeToDaily.Seconds + 60}";
                    }

                }


                string DailyRep = null;
                TimeToDaily = UserDataBase.DailyRep - DateTime.Now;
                if (TimeToDaily.TotalSeconds < -1)
                    DailyRep = "Репутация - доступна сейчас!";
                else
                    DailyRep = $"До репутации - {TimeToDaily.Hours}:{TimeToDaily.Minutes}:{TimeToDaily.Seconds}";

                string Marryed = string.Empty;
                if (UserDataBase.UsersMId != null)
                {
                    string Timemarryed = string.Empty;
                    var Time = DateTime.Now - UserDataBase.UsersMTime;
                    if (Time.TotalSeconds < 60)
                        Timemarryed = "Меньше минуты";
                    else if (Time.TotalMinutes <= 60)
                        Timemarryed = $"{Math.Round(Time.TotalMinutes)} минут";
                    else if (Time.TotalHours <= 24)
                        Timemarryed = $"{Math.Round(Time.TotalHours)} часов";
                    else
                        Timemarryed = $"{Math.Round(Time.TotalDays)} дней";

                    Marryed = $"Половинка: <@{UserDataBase.UsersMId}>\nВ браке: {Timemarryed}";
                    emb.AddField("Отношения", Marryed,true);
                }
                var VoiceActive = UserDataBase.VoiceActive;
                var VoiceHourse = VoiceActive.Hours;
                if (VoiceActive.Days > 0)
                    VoiceHourse += VoiceActive.Days * 24;
                emb.AddField("Репутация", $"Количество: {UserDataBase.Reputation}\n{DailyRep}", true);
                emb.AddField("Звездочки", $"Количество: {UserDataBase.Coins}\nКомбо получения звездочек: {UserDataBase.Streak}\n{DailyCoin}", true);
                emb.AddField("Опыт", $"Уровень: {UserDataBase.Level}\nОпыт: {UserDataBase.XP - count}/{countNext - count}\nАктивность в голосовых чатах: {VoiceHourse}:{VoiceActive.Minutes}:{VoiceActive.Seconds}", false);

                string prefix = _db.Settings.FirstOrDefault().Prefix;
                string MinecraftText = $"Попробуй бесплатно: {prefix}mineget";

                if(UserDataBase.Minecraft_User != null)
                {
                    var SubscribeList = UserDataBase.Minecraft_User.Minecraft_User_Subscribe.OrderBy(x=>x.SubscribeTo);
                    if(SubscribeList.Count() > 0)
                    {
                        MinecraftText = $"Ник: {UserDataBase.Minecraft_User.UserName}\n";
                        var Subscribe = SubscribeList.FirstOrDefault(x => x.ActiveSubscribe);
                        if (Subscribe == null)
                        {
                            Subscribe = SubscribeList.LastOrDefault();
                            MinecraftText += $"Подписка истекла!\nПоследняя подписка до: {Subscribe.SubscribeTo}";
                        }
                        else
                            MinecraftText += $"Подписка до: {Subscribe.SubscribeTo}\nПремиум доступ: {(Subscribe.Premium ? "Да" : "Нет")}";
                    }

                }

                emb.AddField("Minecraft Profile", MinecraftText, true);

                //emb.WithDescription($"Звездочек: {UserDataBase.Coins}\n" +
                //                                             $"Репутация: {UserDataBase.Reputation}\n\n" +
                //                                             $"Комбо получения звездочек: {UserDataBase.Streak}\n" +
                //                                             $"{DailyCoin}\n" +
                //                                             $"{DailyRep}\n\n" +
                //                                             $"Уровень: {UserDataBase.Level}\n" +
                //                                             $"Опыт: {UserDataBase.XP - count}/{countNext - count}\n" +
                //                                             $"Активность в голосовых чатах: {UserDataBase.VoiceActive}\n" +
                //                                             $"{Marryed}");

                
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Command("Starcoin"), Alias("sc")]
        [Summary("Получить звездочки")]
        public async Task StarCoin() => await daily();

        [Command("Rep"), Alias("r")]
        [Summary("Выдать репутацию")]
        public async Task rep(SocketGuildUser User) => await daily(User);


        [Command("reprole"), Alias("rr")]
        [Summary("Репутационные роли")]
        public async Task reprole() => await RoleInfo(RoleTypeEnum.Reputation);


        [Command("levelrole"), Alias("lr")]
        [Summary("Уровневые роли")]
        public async Task levelrole() => await RoleInfo(RoleTypeEnum.Level);

        private async Task RoleInfo(RoleTypeEnum Type)
        {
            using (db _db = new())
            {
                string Translate = "Уровневые";
                if (Type == RoleTypeEnum.Reputation)
                    Translate = "Репутационные";

                var lvl = _db.Roles_Type.Where(x => x.Type == Type).AsEnumerable().OrderBy(u => Convert.ToUInt32(u.Value));
                var embed = new EmbedBuilder().WithAuthor($"🔨 {Translate} роли {(lvl.Any() ? "" : "отсутствуют ⚠️")}")
                                              .WithColor(BotSettings.TsunaColor);

                Translate = "уровень";
                if (Type == RoleTypeEnum.Reputation)
                    Translate = "репутация";

                foreach (var LVL in lvl)
                    embed.Description += $"{LVL.Value} {Translate} - <@&{LVL.RoleId}>\n";

                if (Context.Guild.Owner == Context.User)
                {
                    Translate = "lr";
                    if (Type == RoleTypeEnum.Reputation)
                        Translate = "rr";

                    var Settings = _db.Settings.FirstOrDefault();
                    embed.AddField("Добавить", $"{Settings.Prefix}{Translate}a [ROLE] [{(Type == RoleTypeEnum.Level ? "Level" : "Reputation")}]");
                    embed.AddField("Удалить", $"{Settings.Prefix}{Translate}d [ROLE]");
                }
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        private enum DailyType : byte
        {
            Coin,
            Rep
        }
        private async Task daily(SocketGuildUser User = null)
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($" - {(User != null ? "репутация" : "звездочки")} 🏧", Context.User.GetAvatarUrl());
                var UserThis = await _db.Users.GetOrCreate(Context.User.Id);
                var DateNow = DateTime.Now;

                DateTime Daily = UserThis.DailyCoin;

                if (User != null)
                    Daily = UserThis.DailyRep;

                if (Daily.Year == 1)
                    Daily = DateNow;

                if (DateNow >= Daily)
                {
                    if (User == null)
                    {
                        if ((DateNow - Daily).TotalSeconds >= 18000)
                            UserThis.Streak = 1;
                        else
                            UserThis.Streak++;

                        UserThis.DailyCoin = DateNow.AddHours(5);

                        uint amt = (uint)(500 + ((500 / 35) * UserThis.Streak));
                        if ((UserThis.Coins + amt) > BotSettings.CoinsMaxUser)
                        {
                            UserThis.Coins = BotSettings.CoinsMaxUser;
                            emb.WithDescription($"Звездочек получено: Вы превысили лимит.");
                        }
                        else
                        {
                            UserThis.Coins += amt;
                            emb.WithDescription($"Звездочек получено: {amt}");
                        }
                        emb.Description += $"\nКомбо: {UserThis.Streak}\nСледующее получения звездочек через 5 часов";
                        _db.Users.Update(UserThis);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        if (User.Id == Context.User.Id)
                            emb.WithDescription("Повысить репутацию самому себе нельзя.");
                        else
                        {
                            if (UserThis.LastUserId == 0 || User.Id != UserThis.LastUserId)
                            {
                                var UserRep = await _db.Users.GetOrCreate(User.Id);
                                await RepRole(User, UserRep.Reputation);
                                UserRep.Reputation += 1;
                                UserThis.LastUserId = UserRep.Id;
                                UserThis.DailyRep = DateNow.AddHours(5);
                                
                                emb.WithDescription($"{Context.User.Mention} повысил репутацию {User.Mention}\nРепутация: +{UserRep.Reputation}\nСледующая репутация через 5 часов"); // {Math.Round((UserThis.DailyRep - DateTime.Now).TotalHours)}
                                _db.Users.UpdateRange(new[] { UserRep, UserThis });
                                await _db.SaveChangesAsync();
                            }
                            else
                                emb.WithDescription("Вы не можете выдать репутацию одному и тому же пользователю 2 раза подряд.");
                        }
                    }
                }
                else
                {
                    var TimeToDaily = Daily - DateNow;
                    if (TimeToDaily.TotalSeconds >= 3600)
                        emb.WithDescription($"Дождитесь {TimeToDaily.Hours} часов и {TimeToDaily.Minutes} минут чтобы {(User == null ? "получить звездочки" : "выдать репутацию")}!");
                    else
                        emb.WithDescription($"Дождитесь {(TimeToDaily.TotalSeconds > 60 ? $"{TimeToDaily.Minutes} минут и " : "")} {TimeToDaily.Seconds} секунд чтобы {(User == null ? "получить звездочки" : "выдать репутацию")}!");
                }

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }
        public static async Task RepRole(SocketGuildUser UserDiscord, ulong Reputation)
        {
            using (db _db = new())
            {
                var Roles = _db.Roles_Type.Where(x => x.Type == RoleTypeEnum.Reputation).AsEnumerable().OrderBy(x => x.Value);
                var ThisRole = Roles.LastOrDefault(x => x.Value <= Reputation);

                Reputation++;
                var NextRole = Roles.FirstOrDefault(x => x.Value == Reputation);
                if (NextRole != null)
                {
                    if (ThisRole != null)
                        await UserDiscord.RemoveRole(ThisRole.RoleId);

                    await UserDiscord.AddRole(NextRole.RoleId);
                }
                else if (ThisRole != null)
                    await UserDiscord.AddRole(ThisRole.RoleId);
            }
        }


        [Command("marry"), Alias("ma")]
        [Summary("Обручиться с пользователем")]
        [MarryPermission]
        public async Task marry(SocketGuildUser user)
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($"💞 Женидьба - Ошибка");
                if (Context.User.Id != user.Id)
                {
                    var marryuser = await _db.Users.GetOrCreate(user.Id);
                    if (marryuser.UsersMId == null)
                    {
                        var ContextUser = await _db.Users.GetOrCreate(Context.User.Id);
                        var time = DateTime.Now.AddSeconds(60);
                        emb.WithAuthor($"{Context.User} 💞 {user}").WithDescription($"Заявка на свадьбу, отправлена {user.Mention}").WithFooter("Заявка активна в течении минуты").WithThumbnailUrl(user.GetAvatarUrl());
                        var builder = new ComponentBuilder().WithButton("Принять", nameof(ButtonActionEnum.Marryed_Yes), ButtonStyle.Success).WithButton("Отклонить", nameof(ButtonActionEnum.Marryed_No), ButtonStyle.Danger);
                        var mes = await Context.Channel.SendMessageAsync("", false, emb.Build(), components: builder.Build());
                        var NewApplication = new ActionData { MessageId = mes.Id, UserId = user.Id, Type = ButtonActionEnum.Marryed_Wait };
                        ListData.Add(NewApplication);

                        while (time > DateTime.Now)
                        {
                            NewApplication = ListData.FirstOrDefault(x => x.MessageId == mes.Id);
                            if (NewApplication.Type == ButtonActionEnum.Marryed_Yes)
                            {
                                ContextUser.UsersMId = marryuser.Id;
                                marryuser.UsersMId = ContextUser.Id;
                                marryuser.UsersMTime = DateTime.Now;
                                ContextUser.UsersMTime = DateTime.Now;
                                _db.Users.UpdateRange(new[] { ContextUser, marryuser });
                                await _db.SaveChangesAsync();
                                emb.WithDescription($"{user.Mention} и {Context.User.Mention} поженились!");
                                break;
                            }
                            else if (NewApplication.Type == ButtonActionEnum.Marryed_No)
                            {
                                emb.WithDescription($"{user.Mention} отказался(лась) от свадьбы!");
                                break;
                            }
                        }

                        if (NewApplication.Type == ButtonActionEnum.Marryed_Wait)
                            emb.WithDescription($"{user.Mention} не успел(а) принять заявку!");

                        ListData.Remove(NewApplication);
                        emb.Footer.Text = null;
                        await mes.ModifyAsync(x => { x.Components = new ComponentBuilder().Build(); x.Embed = emb.Build(); });
                        return;
                    }
                    else
                    {
                        var Prefix = _db.Settings.FirstOrDefault();
                        emb.WithDescription($"{user} женат(а), этому пользователю сначала нужно развестись!").WithFooter($"Развестить - {Prefix.Prefix}divorce");
                    } 
                }
                else 
                    emb.WithDescription("Я знаю что ты любишь себя, но к сожалению на себе жениться нельзя!");

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Command("mineget"), Alias("mg")]
        [Summary("Получить доступ к серверу")]
        public async Task mineget()
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($" - Получение доступа ", Context.User.GetAvatarUrl()).WithDescription("");
                var ThisUser = _db.Users.Include(x=>x.Minecraft_User).ThenInclude(x=>x.Minecraft_User_Subscribe).FirstOrDefault(x=>x.Id == Context.User.Id);
                int StartMaxLevel = 15;
                if(ThisUser.Level >= StartMaxLevel)
                {
                    if (ThisUser.Minecraft_User != null && ThisUser.Minecraft_User.RulesAccept)
                    {
                        if(ThisUser.Minecraft_User.UserName == null)
                        {
                            emb.Author.Name += "[2/5]";
                            emb.WithDescription("Сейчас вам должен написать наш Бот, отправьте ему свой ник Minecraft, для привязки к серверу.");
                            try
                            {
                                await Context.User.SendMessageAsync("Привет, отправь сообщение со своим ником Minecraft!");
                            }
                            catch (Exception)
                            {
                                emb.Description += "\n\n" +
                                                "Боту не удалось вам написать:\n" +
                                                "1.Нажмите правой кнопкой мыши по боту\n" +
                                                "2.Нажмите `написать сообщение`";
                                emb.WithImageUrl("https://media.discordapp.net/attachments/1003743130622885908/1005867148666753065/unknown.png");
                            }
                        }
                        else
                        {
                            var ThisSubsTime = ThisUser.Minecraft_User.Minecraft_User_Subscribe.FirstOrDefault(x=>x.ActiveSubscribe);
                            if (ThisSubsTime != null)
                            {
                                var prefix = getprefix();
                                emb.WithDescription($"Ваша подписка активна до `{ThisSubsTime.SubscribeTo.ToString("dd.MM.yy")}`\n\n" +
                                                    $"Хотите продлить подписку больше чем на месяц?\nНапишите {prefix}minebuy");
                            }
                            else
                            {
                                int MaxLevel = StartMaxLevel + (5 * ThisUser.Minecraft_User.CountSubs);

                                if (ThisUser.Level >= MaxLevel)
                                {
                                    DateTime LastTime = DateTime.Now;
                                    var newsubscribe = new Minecraft_User_Subscribe { SubscribeAt = LastTime, Minecraft_User = ThisUser.Minecraft_User };
                                    newsubscribe.SubscribeTo = LastTime.AddMonths(1);
                                    ThisUser.Minecraft_User.Minecraft_User_Subscribe.Add(newsubscribe);
                                    
                                    _db.Users.Update(ThisUser);
                                    await _db.SaveChangesAsync();

                                    await myrcon.SendQuery($"whitelist add {ThisUser.Minecraft_User.UserName}");
                                    await TaskTimer.CheckMinecraftSubscribeNow(ThisUser.Minecraft_User,Context.User as SocketGuildUser);
                                    emb.Author.Name += "[5/5]";
                                    emb.WithDescription("Вы успешно получили бесплатный месяц доступа на сервер.\n\nПозовите своих друзей на наш проект, вы нам очень поможете.");
                                }
                                else
                                {
                                    var prefix = getprefix();
                                    string Leveltext = getleveltext(StartMaxLevel, prefix);
                                    emb.WithDescription(Leveltext);
                                }
                            }
                        }
                    }
                    else
                    {
                        emb.WithDescription($"Перед тем как получить доступ к серверу,\nвы должны прочитать правила: <#{BotSettings.MinecraftRulesChannel}>.")
                           .WithFooter("Как прочитаете, напишите команду еще раз.");
                        emb.Author.Name += "[1/5]";
                    }
                }
                else
                {
                    var prefix = getprefix();
                    string Leveltext = getleveltext(StartMaxLevel, prefix);
                    emb.WithDescription(Leveltext);
                }

                string getprefix() => _db.Settings.FirstOrDefault().Prefix;

                string getleveltext(int levelneed,string prefix) => $"Чтобы получить доступ, нужно иметь {levelneed} уровень.\n" +
                                                      $"На данный момент у вас: {ThisUser.Level} уровень.\n\n" +
                                                      $"Вы можете купить доступ к серверу без уровня.\n" +
                                                      $"Купить - {prefix}minebuy";

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Command("minebuy"), Alias("mb")]
        [Summary("Купить доступ к серверу")]
        [MarryPermission]
        public async Task minebuy(string buy = "некупить")
        {
            using (var _db = new db())
            {
                var channel = Context.Channel;
                var prefix = _db.Settings.FirstOrDefault().Prefix;
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($" - Премиум доступ к Minecraft серверу", Context.User.GetAvatarUrl());
                if (buy == "некупить")
                {
                    emb.WithDescription("Зачем покупать премиум доступ, если я могу играть бесплатно?\n" +
                                    "-Вы будете полностью правы, НО!\n\n" +
                                    "1.Ваши деньги уходят на развитие сервера!\n" +
                                    "2.Благодаря вам сервер будет существовать!\n" +
                                    "3.В ивентах, у вас будет возможность выиграть деньги.\n" +
                                    "4.В дискорде у вас будет роль <@&1003741382164353076>!\n" +
                                    "5.Доступ можно покупать больше чем на 1 месяц!\n" +
                                    "6.Цена всего **100** рублей, зато сколько эмоций!\n\n" +
                                    "Благодаря вашей активности, и денежной помощи, мы сможем\n" +
                                    "создать интересный проект, и вы будете частью этой истории!\n\n" +
                                    $"Готовы стать частью истории? Пишите - `{prefix}minebuy купить`");
                }
                else if (buy == "купить")
                {
                    var ThisUser = _db.Users.Include(x=>x.Minecraft_User).FirstOrDefault(x => x.Id == Context.User.Id);
                    if(ThisUser.Minecraft_User != null && ThisUser.Minecraft_User.RulesAccept)
                    {
                        if (ThisUser.Minecraft_User.UserName == null)
                            emb.WithDescription("Для привязки вашей учетной записи, введите свой ник minecraft, в личные сообщения БОТУ!");
                        else
                        {
                            var bchannel = Context.Guild.GetTextChannel(1003743130622885908);
                            var embb = emb;
                            embb.WithDescription($"{Context.User.Mention}, хочет PREMIUM доступ.\n" +
                                                 $"Ник в Minecraft: {ThisUser.Minecraft_User.UserName}\n" +
                                                 $"Уровень пользователя: {ThisUser.Level}\n" +
                                                 $"Зарегистрирован: {Context.User.CreatedAt.ToString("dd.MM.yy mm:HH")}\n" +
                                                 $"На сервере с: {(Context.User as SocketGuildUser).JoinedAt.Value.ToString("dd.MM.yy mm:HH")}\n" +
                                                 $"Кол-во сообщений: {ThisUser.XP / 10}");
                            await bchannel.SendMessageAsync("", false, embb.Build());
                            emb.WithDescription("Спасибо за желание помочь нам!\n" +
                                            "Ожидайте ответа от пользователя с ролью <@&1003647883377123328> <:cutebunny:962773673834577940>");
                        }
                    }
                    else
                        emb.WithDescription($"Перед тем как купить доступ, вы должны прочитать правила: <#{BotSettings.MinecraftRulesChannel}>.");
                }
                else
                    emb.WithDescription($"Используйте: {prefix}minebuy купить\nЕсли хотите ознакомиться: {prefix}minebuy");

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }

        }

        [Command("divorce"), Alias("di")]
        [Summary("Развестись с пользователем")]
        [MarryPermission]
        public async Task divorce()
        {
            using (db _db = new())
            {
                var ContextUser = await _db.Users.GetOrCreate(Context.User.Id);
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($" - Развод <@{ContextUser.UsersMId}>", Context.User.GetAvatarUrl());
                emb.WithDescription($"Вы успешно развелись с <@{ContextUser.UsersMId}>!");
                var MarryedUser = await _db.Users.GetOrCreate(Convert.ToUInt64(ContextUser.UsersMId));
                ContextUser.UsersMId = null;
                MarryedUser.UsersMId = null;
                _db.Users.UpdateRange(new[] { ContextUser , MarryedUser});
                await _db.SaveChangesAsync();
                

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Command("buyrole"), Alias("br")]
        [Summary("Купить роли")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task buyrole() => await RoleTimer();



        private async Task RoleTimer()
        {
            using (db _db = new())
            {
                var embed = new EmbedBuilder().WithColor(BotSettings.TsunaColor);
                var user = await _db.Users.GetOrCreate(Context.User.Id);
                var DBroles = _db.Roles_Type.Where(x => x.Type == RoleTypeEnum.Buy).AsEnumerable();
                if (DBroles.Any())
                {
                    embed.WithAuthor($"🔨 Покупка ролей");
                    DBroles = DBroles.OrderBy(u => u.Value);
                    int CountSlot = 3;
                    var Id = await ListBuilder.ListButtonSliderBuilder(DBroles, embed,  "buyrole" , Context, CountSlot, true);
                    var Data = ListData.FirstOrDefault(x => x.MessageId == Id.Item1 && x.Type == ButtonActionEnum.Number_Wait ||
                                                                                 x.Type == ButtonActionEnum.Number_1 ||
                                                                                 x.Type == ButtonActionEnum.Number_2 ||
                                                                                 x.Type == ButtonActionEnum.Number_3 ||
                                                                                 x.Type == ButtonActionEnum.Number_4 ||
                                                                                 x.Type == ButtonActionEnum.Number_5);

                    embed.WithFooter("");
                    if (Data.Type != ButtonActionEnum.Number_Wait)
                    {
                        var Number = (Convert.ToInt32(Data.Type.ToString().Split('_')[1]) - 1);
                        var DBrole = DBroles.ToList()[(Id.Item2 - 1) * CountSlot + Number];
                        var Role = Context.Guild.GetRole(DBrole.RoleId);

                        if (user.Coins >= DBrole.Value)
                        {
                            if (!_db.Roles_User.Any(x=>x.RoleId == Role.Id && x.UsersId == Context.User.Id))
                            {
                                _db.Roles_User.Add(new Roles_User { RoleId = Role.Id,UsersId = Context.User.Id });
                                user.Coins -= DBrole.Value;
                                _db.Users.Update(user);
                                await _db.SaveChangesAsync();
                                embed.WithDescription($"Вы успешно купили {Role.Mention} за {DBrole.Value} звездочек");
                            }
                            else
                                embed.WithDescription($"Вы уже купили роль {Role.Mention}");
                            
                            if (!(Context.User as SocketGuildUser).Roles.Contains(Role))
                                await Context.User.AddRole(Role.Id);
                        }
                        else 
                            embed.WithDescription($"У вас недостаточно средств на счете!\nВаш баланс: {user.Coins} звездочек");

                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                    ListData.Remove(Data);
                }
                else
                {
                    if ((Context.User as SocketGuildUser).GuildPermissions.Administrator)
                    {
                        var Prefix = _db.Settings.FirstOrDefault().Prefix;
                        embed.AddField("Добавить роль", $"{Prefix}bra [ROLE] [PRICE] ", true);
                        embed.AddField("Удалить роль", $"{Prefix}brd [ROLE]", true);
                    }
                    else embed.WithDescription($"Попросите администратора сервера выставить роли на продажу <3");

                    await Context.Channel.SendMessageAsync("", false, embed.WithAuthor($"🔨BuyRole - Роли не выставлены на продажу ⚠️").Build());
                }
            }
        }
    }
}
