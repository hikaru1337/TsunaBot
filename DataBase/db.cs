using Microsoft.EntityFrameworkCore;
using TsunaBot.DataBase.Models;
using TsunaBot.Services;

namespace TsunaBot.DataBase
{
    public class db : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Minecraft_User> Minecraft_User { get; set; }
        public DbSet<Minecraft_User_Subscribe> Minecraft_User_Subscribe { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Roles_Type> Roles_Type { get; set; }
        public DbSet<Roles_User> Roles_User { get; set; }
        public DbSet<Settings> Settings { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.UseSqlite(BotSettings.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Settings>().HasData(new Settings() { Id = 1, Prefix = BotSettings.Prefix, BotStatus = "Люблю тсуночку и кранфису <3" });

            modelBuilder.Entity<Users>().HasOne(p => p.Minecraft_User).WithOne(t => t.Users).HasForeignKey<Minecraft_User>(c => c.UsersId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
