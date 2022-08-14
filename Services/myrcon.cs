using CoreRCON;
using System.Net;
using System.Threading.Tasks;

namespace TsunaBot.Services
{
    public static class myrcon
    {
        private static ushort port = 25692;
        private static string IP = "65.21.216.224";
        private static string Key = "Urd?cDHY)K]O/ne2N,/ey-Jzv161Y)$_!|x[Xn6wF№elVrOMZ#W!zk5+j&";

        public static async Task SendQuery(string query, string query2 = null, string query3 = null)
        {
            var rcon = new RCON(IPAddress.Parse(IP), port, Key);
            await rcon.ConnectAsync();
            await rcon.SendCommandAsync(query);
            if (query2 is not null)
                await rcon.SendCommandAsync(query2);
            if (query3 is not null)
                await rcon.SendCommandAsync(query3);
        }

        public static async Task<string> SendQuery(string query)
        {
            var rcon = new RCON(IPAddress.Parse(IP), port, Key);
            await rcon.ConnectAsync();
            var request = await rcon.SendCommandAsync(query);
            return request;
        }
    }
}
