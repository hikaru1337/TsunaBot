using DarlingNet.Services.LocalService.VerifiedAction;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TsunaBot.DataBase;
using TsunaBot.DataBase.Models;

namespace TsunaBot.Services
{
    public class Leveling
    {

        public static async Task LVL(SocketUserMessage Message,Users User)
        {
            using (db _db = new ())
            {
                var UserDiscord = Message.Author as SocketGuildUser;
                var Roles = _db.Roles_Type.Where(x=>x.Type == Roles_Type.RoleTypeEnum.Level).AsEnumerable().OrderBy(x => x.Value);
                var ThisRole = Roles.LastOrDefault(x=> x.Value <= User.Level);

                foreach (var Role in _db.Roles_User.Where(x=>x.UsersId == User.Id))
                {
                    await UserDiscord.AddRole(Role.RoleId);
                }

                await Modules.User.RepRole(UserDiscord,User.Reputation);

                var NextLevel = (ulong)Math.Sqrt((User.XP + 10) / 100);
                if (NextLevel > User.Level)
                {
                    var NextRole = Roles.FirstOrDefault(x => x.Value == NextLevel);
                    if (NextRole != null)
                    {
                        if (ThisRole != null)
                            await UserDiscord.RemoveRole(ThisRole.RoleId);

                        await UserDiscord.AddRole(NextRole.RoleId);
                    }

                    uint amt = (uint)(500 + ((500 / 35) * (User.Level + 1)));
                    var Fields = new EmbedBuilder().WithAuthor("LEVEL UP", UserDiscord.GetAvatarUrl())
                                      .WithColor(BotSettings.TsunaColor)
                                      .AddField("LEVEL", $"{User.Level + 1}", true)
                                      .AddField("XP", $"{User.XP + 10}", true);
                    await Message.Channel.Message("", Fields);

                }
                else if (ThisRole != null)
                    await UserDiscord.AddRole(ThisRole.RoleId);

                User.XP += 10;
                _db.Users.Update(User);
                await _db.SaveChangesAsync();
            }
        }
    }
}
