

namespace TsunaBot.DataBase.Models
{
    public class Roles_User
    {
        public uint Id { get; set; }
        public ulong RoleId { get; set; }
        public Roles Role { get; set; }
        public ulong UsersId { get; set; }
        public Users Users { get; set; }
    }
}
