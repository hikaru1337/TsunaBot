using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DarlingNet.Services.LocalService.Errors
{
    public class Initiliaze
    {
        private static readonly Dictionary<string, Errors> _commandData;
        static Initiliaze()
        {
            _commandData = JsonConvert.DeserializeObject<Dictionary<string, Errors>>(File.ReadAllText("ErrorsList.json"));
        }
        public static Errors Load(string key)
        {
            _commandData.TryGetValue(key, out var toReturn);

            if (toReturn == null)
                toReturn = new Errors { Rus = key, HelpCommand = key };

            return toReturn;
        }

        public class Errors
        {
            public string Rus { get; set; }
            public string HelpCommand { get; set; }
        }
    }
}
