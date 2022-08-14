﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TsunaBot.DataBase;

namespace TsunaBot.Migrations
{
    [DbContext(typeof(db))]
    [Migration("20220430204241_First")]
    partial class First
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0-preview.5.21301.9");

            modelBuilder.Entity("TsunaBot.DataBase.Models.Roles", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("TsunaBot.DataBase.Models.Roles_Type", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Value")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("Roles_Type");
                });

            modelBuilder.Entity("TsunaBot.DataBase.Models.Settings", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BotStatus")
                        .HasColumnType("TEXT");

                    b.Property<string>("Prefix")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Settings");

                    b.HasData(
                        new
                        {
                            Id = 1ul,
                            BotStatus = "Люблю тсуночку и кранфису <3",
                            Prefix = "t."
                        });
                });

            modelBuilder.Entity("TsunaBot.DataBase.Models.Users", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Coins")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DailyCoin")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DailyRep")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("Reputation")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("Streak")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("VoiceActive")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("XP")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TsunaBot.DataBase.Models.Roles_Type", b =>
                {
                    b.HasOne("TsunaBot.DataBase.Models.Roles", "Role")
                        .WithMany("Roles_Type")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("TsunaBot.DataBase.Models.Roles", b =>
                {
                    b.Navigation("Roles_Type");
                });
#pragma warning restore 612, 618
        }
    }
}
