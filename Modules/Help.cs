using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TsunaBot.DataBase;
using TsunaBot.Services;

namespace DarlingNet.Modules
{
    [Summary("Управление ботом")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RequireBotPermission(GuildPermission.SendMessages)]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IServiceProvider _provider;


        public Help(CommandService service, IServiceProvider provider)
        {
            _service = service;
            _provider = provider;
        }

        [Command("modules"), Alias("m")]
        [Summary("Список модулей")]
        public async Task modules()
        {
            using (db _db = new ())
            {
                var settings = _db.Settings.FirstOrDefault();
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor("📚 Модули бота")
                                            .WithFooter($"Команды модуля - {settings.Prefix}c [Модуль]");
                var mdls = _service.Modules;

                foreach (var mdl in mdls)
                {
                    var Permission = await mdl.GetExecutableCommandsAsync(Context, _provider);
                    if (Permission.Count > 0)
                    {
                        emb.AddField(mdl.Name, mdl?.Summary, true);
                    }
                }
                if (emb.Fields.Count == 0) 
                    emb.WithDescription("Модули бота отсутствуют!");
                await Context.Channel.SendMessageAsync("",false, emb.Build());
            }
        }

        [Command("commands"), Alias("c")]
        [Summary("Список команд модуля")]
        public async Task commands(string modules)
        {
            using (db _db = new ())
            {
                var settings = _db.Settings.FirstOrDefault();
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithDescription("").WithAuthor($"📜 {modules} - Команды [префикс - {settings.Prefix}]");

                var mdls = _service.Modules.FirstOrDefault(x => x.Name.ToLower() == modules.ToLower());
                if (mdls != null)
                {
                    var SuccessCommands = await mdls.GetExecutableCommandsAsync(Context, _provider);
                    foreach (var Command in SuccessCommands)
                        emb.Description += $"• {Command.Aliases[1]} [{Command.Aliases[0]}]\n";
                    
                    emb.WithFooter($"Подробная информация о команде - {settings.Prefix}i [Имя команды]");
                }
                else emb.WithDescription($"Модуль {modules} не найден!").WithAuthor($"📜{modules} - ошибка");
                await Context.Channel.SendMessageAsync("",false, emb.Build());
            }
        }

        [Command("info"), Alias("i")]
        [Summary("Информация о команде")]
        public async Task info(string command)
        {
            using (db _db = new ())
            {
                command = command.ToLower();
                var Command = _service.Commands.FirstOrDefault(x => x.Aliases[0].ToLower() == command || x.Aliases.Last().ToLower() == command);
                var emb = new EmbedBuilder().WithAuthor($"📋 Информация о {command}").WithColor(BotSettings.TsunaColor);

                if (Command != null)
                {
                    var settings = _db.Settings.FirstOrDefault();
                    string text = string.Empty;
                    foreach (var Parameter in Command.Parameters)
                    {
                        text += $"[{Parameter}{(Parameter.IsOptional ? "/null" : "")}] ";
                    }
                    emb.AddField($"Сокращение: {string.Join(",", Command.Aliases)}",
                                 $"Описание: {Command.Summary}\n" +
                                 $"Пример: {settings.Prefix}{Command.Name} {text}");
                }
                else 
                    emb.WithDescription($"Команда `{command}` не найдена!");

                await Context.Channel.SendMessageAsync("", false ,emb.Build());
            }
        }


        [Command("bot"), Alias("b")]
        [Summary("Информация о боте")]
        public async Task bot()
        {
            var bot = Context.Client.CurrentUser;
            var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor($"Информация о боте {bot.Username}🌏", bot.GetAvatarUrl())
                                                              .WithDescription($"Префикс: {BotSettings.Prefix}\n" +
                                                                               $"Версия: {BotSettings.Version}\n" +
                                                                               $"Заказать бота: https://discord.gg/RazDmcz");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }
    }
}
