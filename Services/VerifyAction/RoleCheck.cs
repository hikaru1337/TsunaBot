using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DarlingNet.Services.LocalService.VerifiedAction
{
    public static class RoleCheck
    {
        public static async Task AddRole(this SocketGuildUser User, ulong RoleId)
            => await CheckRole(User, RoleId, true);

        public static async Task RemoveRole(this SocketGuildUser User, ulong RoleId)
            => await CheckRole(User, RoleId, false);

        public static async Task AddRole(this SocketUser User, ulong RoleId)
            => await CheckRole(User as SocketGuildUser, RoleId, true);

        public static async Task RemoveRole(this SocketUser User, ulong RoleId) 
            => await CheckRole(User as SocketGuildUser, RoleId, false);

        private static async Task CheckRole(SocketGuildUser User, ulong RoleId, bool Add)
        {
            var DiscordRole = User.Guild.GetRole(RoleId);
            if (DiscordRole != null)
            {
                var rolepos = DiscordRole.Guild.CurrentUser.Roles.FirstOrDefault(x => x.Position > DiscordRole.Position);
                if (rolepos != null)
                {
                    if (!DiscordRole.IsManaged)
                    {
                        var bot = User.Guild.CurrentUser;
                        if (bot.GuildPermissions.ManageRoles || bot.GuildPermissions.Administrator)
                        {
                            if (!Add && User.Roles.Contains(DiscordRole))
                                await User.RemoveRoleAsync(DiscordRole);
                            else if (Add && !User.Roles.Contains(DiscordRole))
                                await User.AddRoleAsync(DiscordRole);
                        }
                    }
                }
            }

        }

    }
}
