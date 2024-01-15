using Newtonsoft.Json;
using Server.Tables;
using Supabase;
using static Postgrest.Constants;

namespace Server.Database
{
    static class Database
    {
        static SupabaseOptions options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        static Client? supabase;

        public async static Task DBinit()
        {
            supabase = new Client(Environment.GetEnvironmentVariable("SUPABASE_URL")!, Environment.GetEnvironmentVariable("SUPABASE_KEY"), options);
            await supabase.InitializeAsync();
        }

        async static public Task<Users> GetUserIdByEmail(string email)
        {
            var result = await supabase!
                .From<Users>()
                .Select(y => (new object[] { y.Id }))
                .Where(y => y.Email == email)
                .Single();

            return result!;
        }

        async static public Task UpdateAuthStatus(int id, bool auth)
        {
            var value = await supabase!
                    .From<Users>()
                    .Where(x => x.Id == id)
                    .Set(x => x.AuthenticationStatus, auth)
                    .Update();
        }

        async static public Task<Users> GetUserByEmail(string email)
        {
            var result = await supabase!
                .From<Users>()
                .Select(x => (new object[] { x.Id, x.Avatar!, x.LastLogin!, x.Email!, x.Username! }))
                .Where(x => x.Email == email)
                .Single();

            return result!;
        }

        async static public Task<Users> GetUserByUsername(string Username)
        {
            var result = await supabase!.From<Users>().Select(x => new object[] { x.Id, x.Avatar!, x.LastLogin!, x.Email!, x.Username! }).Where(x => x.Username == Username).Single();

            return result!;
        }

        async static public Task<Postgrest.Responses.ModeledResponse<UserChatUsers>> CreateChatConnection(List<UserChatUsers> models)
        {
            return await supabase!.From<UserChatUsers>().Insert(models);
        }

        async static public Task<Users> GetUserById(int id)
        {
            var result = await supabase!
                .From<Users>()
                .Select(x => (new object[] { x.Id, x.Avatar!, x.LastLogin!, x.Email!, x.Username! }))
                .Where(x => x.Id == id)
                .Single();

            return result!;
        }

        async static public Task<string> GetChatsByUserId(int Id)
        {
            var userChatIdList = JsonConvert.DeserializeObject<List<UserChatUsers>>((await supabase!
                .From<UserChatUsers>()
                .Select(x => (new object[] { x.UserChatId }))
                .Where(x => x.UserId == Id)
                .Get()).Content!)!.Select(x => x.UserChatId).Cast<object>().ToList();

            var result = await supabase
                .From<UserChatUsers>()
                .Select("Users:UserId(Id, Username, Avatar, LastLogin, Email), UserChats:UserChatId(Id, ChatType, ChatName, Avatar), Messages:LastMessage(Message, Users:SenderId(Username), Time)")
                .Filter(x => x.UserChatId, Operator.In, userChatIdList)
                .Where(x => x.UserId != Id)
                .Order("Messages", "Time", Ordering.Descending)
                .Get();

            return result.Content!;
        }

        async static public Task SetNewLastLoginById(int userId, DateTimeOffset time)
        {
            var value = await supabase!
                    .From<Users>()
                    .Where(x => x.Id == userId)
                    .Set(x => x.LastLogin!, time)
                    .Update();
        }
        async static public Task<string> GetContactsIdsByChatId(int chatId, int userId)
        {
            var chat = await supabase!
                .From<UserChatUsers>()
                .Select("Users:UserId(Id)")
                .Where(x => x.UserChatId == chatId && x.UserId != userId)
                .Get();

            return chat!.Content!;
        }

        async static public Task<Users> GetUserByEmailAndPassword(string email, string password)
        {
            var result = await supabase!
                .From<Users>()
                .Select(x => new object[] { x.Id, x.Username!, x.Email!, x.AuthenticationStatus, x.Avatar! })
                .Where(x =>
                        x.Email == email &&
                        x.Password == password
                        )
                .Single();

            return result!;
        }

        async static public Task<Users> InsertUserToTableUsers(Users user)
        {
            return (await supabase!.From<Users>().Select(x => new object[] { x.Id, x.Username!, x.Email!, x.AuthenticationStatus }).Insert(user!)).Model!;
        }

        async static public Task<HttpResponseMessage> UpdateAuthByEmail(string email, bool auth)
        {
            var value = await supabase!
                .From<Users>()
                .Where(x => x.Email == email)
                .Set(x => x.AuthenticationStatus, auth)
                .Update();

            return value.ResponseMessage!;
        }

        async static public Task InsertMessageToTableMessages(Messages message)
        {
            _ = await supabase!.From<Messages>().Insert(message!);
        }

        async static public Task<HttpResponseMessage> UpdatePassword(Users myObject)
        {
            var value = await supabase!
                        .From<Users>()
                        .Where(x => x.Email == myObject.Email)
                        .Set(x => x.AuthenticationStatus, false)
                        .Set(x => x.Password!, myObject.Password)
                        .Update();

            return value.ResponseMessage!;
        }

        async static public Task<string> GetMessagesByChatId(int chatId)
        {
            var messages = await supabase!
                    .From<Messages>()
                    .Select("Users:SenderId(Username), Time, Message")
                    .Where(x => x.UserChatId == chatId)
                    .Get();

            return messages.Content!;
        }

        async static public Task<Postgrest.Responses.ModeledResponse<UserChats>> CreateNewChat()
        {
            return await supabase!.From<UserChats>().Insert(new UserChats { ChatType = ChatType.Chat });
        }

        async static public Task<Postgrest.Responses.ModeledResponse<Users>> FindUsersByUsername(string Username, int Id)
        {
            return await supabase!.From<Users>()
                  .Filter(x => x.Username!, Operator.ILike, Username + "%")
                  .Where(x => x.Id != Id)
                  .Limit(5)
                  .Get();
        }
    }
}
