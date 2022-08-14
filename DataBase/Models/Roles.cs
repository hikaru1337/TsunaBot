using System.Collections.Generic;

namespace TsunaBot.DataBase.Models
{
    public class Roles
    {
        public ulong Id { get; set; }
        public ICollection<Roles_Type> Roles_Type { get; set; }
        public ICollection<Roles_User> Roles_User { get; set; }
    }
}