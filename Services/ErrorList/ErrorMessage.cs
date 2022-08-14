using TsunaBot.DataBase;
using TsunaBot.DataBase.Models;
using DarlingNet.Services.LocalService.Errors;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TsunaBot.Services;

namespace DarlingNet.Services.LocalService
{
    public class ErrorMessage
    {
        public static EmbedBuilder GetError(string error, string prefix, CommandInfo command = null)
        {
            using (db _db = new ())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.TsunaColor).WithAuthor("Ошибка!");
                string text = string.Empty;
                var SetError = Initiliaze.Load(error);

                if (SetError?.Rus != error)
                {
                    if (SetError.HelpCommand == "true")
                    {
                        foreach (var Parameter in command.Parameters)
                        {
                            if (Parameter.IsOptional)
                                text += $"[{Parameter}/может быть пустым]";
                            else if (Parameter.IsRemainder)
                                text += $"[{Parameter}/поддерживает предложения]";
                            else
                                text += $"[{Parameter}] ";
                        }

                        emb.WithDescription($"Описание ошибки: " + SetError.Rus);
                        emb.AddField($"Описание команды: ", $"{command.Summary ?? "отсутствует"}", true);
                        emb.AddField("Пример команды:", $"{prefix}{command.Name} {text}");
                    }
                    else
                        emb.WithDescription($"{SetError.Rus}");
                }
                else emb.WithDescription(error);

                return emb;
            }
        }
    }
}
