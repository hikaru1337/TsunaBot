using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Timers;
using TsunaBot.DataBase;
using TsunaBot.Services.GetOrCreate;

namespace TsunaBot.Services
{
    internal class VoiceActive
    {
        public static async Task StartVoicesActivity(SocketGuild Guild)
        {
            foreach (var VoiceChannel in Guild.VoiceChannels)
            {
                if (VoiceChannel.Users.Count > 0)
                {
                    foreach (var User in VoiceChannel.Users)
                    {
                        await StartVoiceActivity(User);
                    }
                }
            }
        }

        internal static Task StartVoiceActivity(SocketGuildUser User)
        {
            Timer TaskTime = new(10000);
            TaskTime.Elapsed += (s, e) => VoiceActivity(s, User);
            TaskTime.Start();

            return Task.CompletedTask;
        }
        private static async void VoiceActivity(object source, SocketGuildUser User)
        {
            using (db _db = new())
            {
                if (User.VoiceChannel != null && User.VoiceChannel.Id != User.Guild.AFKChannel?.Id)
                {
                    if (User.VoiceChannel.Users.Count > 1)
                    {
                        uint CountSpeak = 0;
                        bool ThisUserActive = false;
                        foreach (var UserChannel in User.VoiceChannel.Users)
                        {
                            var UserStatus = UserChannel.VoiceState;
                            if (UserStatus != null)
                            {
                                if (!UserStatus.Value.IsMuted && !UserStatus.Value.IsDeafened &&
                                !UserStatus.Value.IsSelfMuted && !UserStatus.Value.IsSelfDeafened)
                                {
                                    CountSpeak++;

                                    if (UserChannel.Id == User.Id)
                                        ThisUserActive = true;
                                }
                            }
                        }

                        if (ThisUserActive && CountSpeak > 1)
                        {
                            var user = await _db.Users.GetOrCreate(User.Id);
                            user.VoiceActive += new TimeSpan(0, 0, 10);
                            user.XP += 10;
                            _db.Users.Update(user);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
                else
                    (source as Timer).Stop();

            }
        }
    }
}
