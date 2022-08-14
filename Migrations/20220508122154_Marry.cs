using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TsunaBot.Migrations
{
    public partial class Marry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "UsersMId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "UsersMId1",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UsersMTime",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Users_UsersMId1",
                table: "Users",
                column: "UsersMId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_UsersMId1",
                table: "Users",
                column: "UsersMId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_UsersMId1",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UsersMId1",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UsersMId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UsersMId1",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UsersMTime",
                table: "Users");
        }
    }
}
