using Newtonsoft.Json;

namespace Server
{
    class Start
    {
        static async Task Main(string[] args)
        {
            await Database.Database.DatabaseInit(); // Init database
                                                    // Первый список
            string firstListJson = "[{\"Chat\": 30, \"CountOfUnreadMessages\": 10}]";
            var firstList = JsonConvert.DeserializeObject<List<Dictionary<string, int>>>(firstListJson);

            // Второй список
            string secondListJson = "[{\"Messages\":{\"Time\": \"2024-02-06T19:46:40.360184+00:00\", \"Users\": {\"Username\": \"Dimi\"}, \"Message\": \"1\"},\"Users\":{\"Id\": 3, \"Email\": \"Dodo1@gmail.com\", \"Avatar\": null, \"Username\": \"Dodo\", \"LastLogin\": \"2024-02-05T23:57:48.499603+00:00\"},\"Chats\":{\"Id\": 30, \"Avatar\": null, \"ChatName\": null, \"ChatType\": 1}}, {\"Messages\":{\"Time\": \"2024-02-06T20:15:27.403244+00:00\", \"Users\": {\"Username\": \"Dimi\"}, \"Message\": \"tetetetet\"},\"Users\":{\"Id\": 3, \"Email\": \"Dodo1@gmail.com\", \"Avatar\": null, \"Username\": \"Dodo\", \"LastLogin\": \"2024-02-05T23:57:48.499603+00:00\"},\"Chats\":{\"Id\": 31, \"Avatar\": null, \"ChatName\": \"TESTTTTT\", \"ChatType\": 2}}]";
            var secondList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(secondListJson);

            // Создаем словарь для быстрого доступа к CountOfUnreadMessages по Id чата
            Dictionary<int, int> chatIdToUnreadMessages = new Dictionary<int, int>();
            foreach (var item in firstList)
            {
                chatIdToUnreadMessages[item["Chat"]] = item["CountOfUnreadMessages"];
            }

            // Обновляем второй список с учетом CountOfUnreadMessages
            foreach (var item in secondList)
            {
                var chat = (Dictionary<string, object>)item["Chats"];
                int chatId = (int)chat["Id"];
                if (chatIdToUnreadMessages.ContainsKey(chatId))
                {
                    chat["CountOfUnreadMessages"] = chatIdToUnreadMessages[chatId];
                }
            }

            // Печатаем обновленный второй список
            foreach (var item in secondList)
            {
                Console.WriteLine(JsonConvert.SerializeObject(item));
            }
            await Database.Database.GetChatsByUserId(1);
            await Server.Server.StartServerAsync(); // start server
        }
    }
}
