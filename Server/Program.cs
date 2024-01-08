
namespace Server
{
    class Start
    {
        static async Task Main(string[] args)
        {
            await Database.Database.DBinit(); // Init database
            await Server.Server._startServerAsync(); // start server
        }
    }
}
