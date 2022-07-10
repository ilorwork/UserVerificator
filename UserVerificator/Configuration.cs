using Newtonsoft.Json;
using System.IO;

namespace UserVerificator
{
    internal class Configuration
    {
        public string botToken;
        public string logChatId;

        public static Configuration LoadConfiguration()
        {
            using (StreamReader r = new StreamReader("userVerificatorConfig.json"))
            {
                string json = r.ReadToEnd();
                Configuration config = JsonConvert.DeserializeObject<Configuration>(json);

                return config;
            }
        }
    }
}
