using Microsoft.EntityFrameworkCore.Migrations;

namespace TsunaBot.Migrations
{
    public partial class AddMinecraftVerify02 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UserNameConfirmed",
                table: "Minecraft_User",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserNameConfirmed",
                table: "Minecraft_User");
        }
    }
}
