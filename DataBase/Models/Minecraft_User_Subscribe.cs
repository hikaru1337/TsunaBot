using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TsunaBot.DataBase.Models
{
    public class Minecraft_User_Subscribe
    {
        public ulong Id { get; set; }
        public ulong Minecraft_UserId { get; set; }
        public Minecraft_User Minecraft_User { get; set; }
        public DateTime SubscribeAt { get; set; }
        public DateTime SubscribeTo { get; set; }
        public bool Premium { get; set; }
        public ulong SubscribeGivenAdminId { get; set; }


        [NotMapped]
        public bool ActiveSubscribe
        {
            get => SubscribeAt < DateTime.Now && SubscribeTo > DateTime.Now;
        }
        [NotMapped]
        public bool SubscribeFuture
        {
            get => SubscribeAt >= DateTime.Now;
        }
    }
}
