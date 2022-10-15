using DarlingNet.Services.LocalService;
using DarlingNet.Services.LocalService.VerifiedAction;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TsunaBot.DataBase;
using TsunaBot.DataBase.Models;
using TsunaBot.Services.GetOrCreate;

namespace TsunaBot.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;

        public CommandHandler(DiscordSocketClient discord, CommandService commands, IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;

            _commands.CommandExecuted += CommandErrors;
            _discord.MessageReceived += SendMessage;
            _discord.UserVoiceStateUpdated += UserVoiceAction;
            _discord.ButtonExecuted += ButtonHandler;
            _discord.Ready += _discord_Ready;
            _discord.RoleDeleted += _discord_RoleDeleted;
        }


        private async Task _discord_RoleDeleted(SocketRole DeletedRole)
        {
            using (var _db = new db())
            {
                var Role = _db.Roles.FirstOrDefault(x=>x.Id == DeletedRole.Id);
                if(Role != null)
                {
                    _db.Remove(Role);
                    await _db.SaveChangesAsync();
                }
            }
        }

        public class ActionData
        {
            public ulong UserId { get; set; }
            public ulong MessageId { get; set; }
            public ButtonActionEnum Type { get; set; }
        }

        public readonly static List<ActionData> ListData = new();
        public enum ButtonActionEnum : byte
        {
            Marryed_Wait,
            Marryed_Yes,
            Marryed_No,
            LeftRight_Wait,
            LeftRight_Left,
            LeftRight_Right,
            Number_Wait,
            Number_1,
            Number_2,
            Number_3,
            Number_4,
            Number_5
        } 
        private async Task ButtonHandler(SocketMessageComponent Button)
        {
            using (db _db = new())
            {
                _ = Enum.TryParse(Button.Data.CustomId, out ButtonActionEnum myStatus);
                var result = ListData.FirstOrDefault(x => x.MessageId == Button.Message.Id && x.Type.ToString().Contains(Button.Data.CustomId.Split('_')[0]));
                if (result != null)
                {
                    if (result.UserId == Button.User.Id)
                    {
                        switch (result.Type)
                        {
                            case ButtonActionEnum.Marryed_Wait:
                                result.Type = myStatus;
                                break;
                            case ButtonActionEnum.Number_Wait:
                                result.Type = myStatus;
                                break;
                            case ButtonActionEnum.LeftRight_Wait:
                                result.Type = myStatus;
                                break;
                        }
                    }
                }
                else
                    await Button.Message.ModifyAsync(x => x.Components = new ComponentBuilder().Build());

                await Button.DeferAsync();
            }
        }

        private async Task _discord_Ready()
        {
            using (var _db = new db())
            {
                var Guild = _discord.Guilds.FirstOrDefault();
                await VoiceActive.StartVoicesActivity(Guild);
                foreach (var Role in _db.Roles)
                {
                    var DiscordRole = Guild.GetRole(Role.Id);
                    if (DiscordRole != null)
                    {
                        _db.Remove(Role);
                        await _db.SaveChangesAsync();
                    }
                }
                Console.WriteLine("Shard Ready -------------------------------------------------------");
            }
            
        }

        private async Task CommandErrors(Optional<CommandInfo> Command, ICommandContext Context, IResult Result)
        {
            using (db _db = new())
            {
                if (!string.IsNullOrWhiteSpace(Result?.ErrorReason))
                {
                    var Settings = _db.Settings.FirstOrDefault();
                    var emb = ErrorMessage.GetError(Result.ErrorReason, Settings.Prefix, Command.IsSpecified ? Command.Value : null);
                    await Context.Channel.Message("", emb);
                }
            }
        }

        private async Task UserVoiceAction(SocketUser User, SocketVoiceState ActionBefore, SocketVoiceState ActionAfter)
        {
            using (db _db = new())
            {
                var UserGuild = User as SocketGuildUser;
                if (ActionAfter.VoiceChannel != null && ActionBefore.VoiceChannel == null)
                    await VoiceActive.StartVoiceActivity(UserGuild);
            }
        }

        private async Task SendMessage(SocketMessage message)
        {
            using (db _db = new())
            {
                if (message is not SocketUserMessage UserMessage ||
                    message.Author.IsBot ||
                    message.MentionedUsers.Any(x=>x.IsBot))
                    return;

                if (message.Channel is not IDMChannel DMChannel)
                {
                    var Context = new SocketCommandContext(_discord, UserMessage);
                    var UserGuild = await _db.Users.GetOrCreate(UserMessage.Author.Id);
                    var Settings = _db.Settings.FirstOrDefault();

                    await Leveling.LVL(UserMessage, UserGuild); // User LevelAdd

                    
                    int argPos = 0;
                    if (UserMessage.HasStringPrefix(Settings.Prefix, ref argPos, StringComparison.InvariantCultureIgnoreCase))
                        await _commands.ExecuteAsync(Context, argPos, _provider);
                }
            }
        }
    }
}
