using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TsunaBot.Migrations
{
    public partial class AddMinecraftVerify : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "Minecraft_UserId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.CreateTable(
                name: "Minecraft_User",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    UsersId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    RulesAccept = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Minecraft_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Minecraft_User_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            //migrationBuilder.CreateTable(
            //    name: "Roles_User",
            //    columns: table => new
            //    {
            //        Id = table.Column<uint>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        RoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
            //        UsersId = table.Column<ulong>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Roles_User", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Roles_User_Roles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "Roles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_Roles_User_Users_UsersId",
            //            column: x => x.UsersId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            migrationBuilder.CreateTable(
                name: "Minecraft_User_Subscribe",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Minecraft_UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    SubscribeAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubscribeTo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Premium = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Minecraft_User_Subscribe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Minecraft_User_Subscribe_Minecraft_User_Minecraft_UserId",
                        column: x => x.Minecraft_UserId,
                        principalTable: "Minecraft_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Minecraft_User_UsersId",
                table: "Minecraft_User",
                column: "UsersId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Minecraft_User_Subscribe_Minecraft_UserId",
                table: "Minecraft_User_Subscribe",
                column: "Minecraft_UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Roles_User_RoleId",
            //    table: "Roles_User",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Roles_User_UsersId",
            //    table: "Roles_User",
            //    column: "UsersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Minecraft_User_Subscribe");

            //migrationBuilder.DropTable(
            //    name: "Roles_User");

            migrationBuilder.DropTable(
                name: "Minecraft_User");

            migrationBuilder.DropColumn(
                name: "Minecraft_UserId",
                table: "Users");
        }
    }
}
