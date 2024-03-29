﻿using Newtonsoft.Json;

namespace Server
{
    class Start
    {
        static async Task Main(string[] args)
        {
            await Database.Database.DatabaseInit(); // Init database
            await Server.Server.StartServerAsync(); // start server
        }
    }
}
