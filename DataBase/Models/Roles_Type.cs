namespace TsunaBot.DataBase.Models
{
    public class Roles_Type
    {
        public ulong Id { get; set; }
        public ulong RoleId { get; set; }
        public Roles Role { get; set; }
        public ulong Value { get; set; }
        public RoleTypeEnum Type { get; set; }

        public enum RoleTypeEnum : byte
        {
            Level,
            Reputation,
            Buy,
        } 
    }
}
