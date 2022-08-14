using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DarlingNet.Services.LocalService.VerifiedAction
{
    public static class SendChannelMessage
    {
        public static async Task Message(this SocketTextChannel Channel, string paintext, EmbedBuilder Build = null)
            => await SendMessageForChannel(Channel, paintext, Build);

        public static async Task Message(this ISocketMessageChannel Channel, string paintext, EmbedBuilder Build = null)
            => await SendMessageForChannel(Channel as SocketTextChannel, paintext, Build);

        public static async Task Message(this IMessageChannel Channel, string paintext, EmbedBuilder Build = null)
            => await SendMessageForChannel(Channel as SocketTextChannel, paintext, Build);

        private static async Task SendMessageForChannel(SocketTextChannel Channel, string paintext, EmbedBuilder Build, MessageComponent Component = null)
        {
            if ((!string.IsNullOrWhiteSpace(paintext) || Build != null) && Channel != null)
            {
                var Bot = Channel.Guild.CurrentUser;
                var ChannelPermEveryOne = Channel.GetPermissionOverwrite(Channel.Guild.EveryoneRole);
                var ChannelPermBot = Channel.GetPermissionOverwrite(Bot);
                if (Bot.GuildPermissions.Administrator ||
                    (ChannelPermEveryOne != null && ChannelPermEveryOne.Value.SendMessages == PermValue.Allow) ||
                    (ChannelPermBot != null && ChannelPermBot.Value.SendMessages == PermValue.Allow))
                {
                    if (Build != null)
                        await Channel.SendMessageAsync(paintext, embed: Build.Build(), components: Component);
                    else
                        await Channel.SendMessageAsync(paintext, components: Component);
                }

            }
        }
    }
}
