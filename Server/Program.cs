namespace Server
{
    class Start
    {
        static async Task Main(string[] args)
        {
            await Database.Database.DatabaseInit(); // Init database
            await Database.Database.GetChatsByUserId(1);
            await Server.Server.StartServerAsync(); // start server
        }
    }
}
