using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TsunaBot.DataBase.Models
{
    public class Users
    {
        public ulong Id { get; set; }
        public TimeSpan VoiceActive { get; set; }
        public ulong Coins { get; set; }
        public ulong XP { get; set; }
        public ushort Level => (ushort)Math.Sqrt(XP / PointFactor);

        [NotMapped]
        public static uint PointFactor = 100;

        public DateTime DailyCoin { get; set; }
        public DateTime DailyRep { get; set; }
        public ushort Streak { get; set; }
        public ulong Reputation { get; set; }
        public ulong LastUserId { get; set; }
        public ulong? UsersMId { get; set; }
        public Users UsersM { get; set; }
        public DateTime UsersMTime { get; set; }
        public ICollection<Roles_User> Roles_User { get; set; }
    }
}
