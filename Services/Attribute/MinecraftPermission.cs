using Discord.Commands;
using System.Threading.Tasks;
using System;
using TsunaBot.DataBase;
using TsunaBot.Services.GetOrCreate;
using Discord.WebSocket;
using System.Linq;

namespace DarlingNet.Services.LocalService.Attribute
{
    sealed class MinecraftPermission : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            bool RoleAccept = false;

            var MineSupport = context.Guild.GetRole(1003650236968870020);

            if ((context.User as SocketGuildUser).Roles.Contains(MineSupport))
                RoleAccept = true;

            if (context.User.Id != 633057226361274403 && context.User.Id != 551373471536513024 && context.User.Id != 308965130262282241 && !RoleAccept)
                return await Task.FromResult(PreconditionResult.FromError($"Введенная команда не найдена!"));


            return await Task.FromResult(PreconditionResult.FromSuccess());
        }

    }
}
