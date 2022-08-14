using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TsunaBot.DataBase;
using TsunaBot.DataBase.Models;

namespace TsunaBot.Services.GetOrCreate
{
    public static class GOCUser
    {
        public static Task<Users> GetOrCreate(this DbSet<Users> Us, ulong UsersId)
        {
            return GetOrCreateUsers(UsersId);
        }
        private static async Task<Users> GetOrCreateUsers(ulong UsersId)
        {
            using (db _db = new ())
            {
                var User = _db.Users.FirstOrDefault(x => x.Id == UsersId);
                if (User == null)
                {
                    User = new Users() { Id = UsersId, Coins = 500 };
                    _db.Users.Add(User);
                    await _db.SaveChangesAsync();
                }

                return User;
            }
        }
    }
}
