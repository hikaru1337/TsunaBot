using Discord.Commands;
using System.Threading.Tasks;
using System;
using TsunaBot.DataBase;
using TsunaBot.Services.GetOrCreate;

namespace DarlingNet.Services.LocalService.Attribute
{
    sealed class MarryPermission : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using (db _db = new ())
            {
                var UserMarry = await _db.Users.GetOrCreate(context.User.Id);
                
                if(command.Name == "marry" && UserMarry.UsersMId != null)
                    return await Task.FromResult(PreconditionResult.FromError($"Вы не можете жениться, так как вы уже женаты!"));
                else if (command.Name == "divorce" && UserMarry.UsersMId == null)
                    return await Task.FromResult(PreconditionResult.FromError($"Вы не можете развестись, так как вы не женаты!"));


                return await Task.FromResult(PreconditionResult.FromSuccess());
            }
        }

    }
}
