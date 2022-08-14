using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TsunaBot.DataBase.Models
{
    public class Minecraft_User
    {
        public ulong Id { get; set; }
        public string UserName { get; set; }
        public bool UserNameConfirmed { get; set; }
        public ulong UsersId { get; set; }
        public Users Users { get; set; }
        public List<Minecraft_User_Subscribe> Minecraft_User_Subscribe { get; set; } = new List<Minecraft_User_Subscribe>();

        [NotMapped]
        public int CountSubs
        {
            get => Minecraft_User_Subscribe.Count;
        }

        public bool RulesAccept { get; set; }
    }
}
