using Newtonsoft.Json;
using System.IO;

namespace UserVerificator
{
    internal class Configuration
    {
        // Your bot's token - load from userVerificatorConfig.json
        public string botToken;

        // Chat id which you want the logs to be sent to(Optional) - load from userVerificatorConfig.json
        public string logChatId;

        // The Max Allowed delay from the moment user join the group - to the moment this server catch that "user join" message.
        // Default value is "5" (in Minutes).
        public string serverDelay = "5";

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
