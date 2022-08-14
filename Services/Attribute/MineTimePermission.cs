using Discord.Commands;
using System.Threading.Tasks;
using System;
using TsunaBot.DataBase;
using TsunaBot.Services.GetOrCreate;
using Discord.WebSocket;
using System.Linq;

namespace DarlingNet.Services.LocalService.Attribute
{
    sealed class MineTimePermission : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            bool RoleAccept = false;

            var MineSupport = context.Guild.GetRole(1003650236968870020);

            if ((context.User as SocketGuildUser).Roles.Contains(MineSupport))
                RoleAccept = true;

            if (RoleAccept || (DateTime.Now.Month >= 08 && DateTime.Now.Day >= 10 && DateTime.Now.Hour >= 15))
                return await Task.FromResult(PreconditionResult.FromSuccess());


            return await Task.FromResult(PreconditionResult.FromError($"Введенная команда не найдена!"));
        }
    }
}
