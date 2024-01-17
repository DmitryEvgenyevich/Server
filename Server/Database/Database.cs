using Newtonsoft.Json;
using Server.Tables;
using Supabase;
using static Postgrest.Constants;

namespace Server.Database
{
    static class Database
    {
        private static SupabaseOptions options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        private static Client? _supabase;

        public static async Task DatabaseInit()
        {
            _supabase = new Client(Environment.GetEnvironmentVariable("SUPABASE_URL")!, Environment.GetEnvironmentVariable("SUPABASE_KEY"), options);
            await _supabase.InitializeAsync();
        }

        public static async Task<Users> GetUserIdByEmail(string email)
        {
            var result = await _supabase!
                .From<Users>()
                .Select(y => (new object[] { y.Id }))
                .Where(y => y.Email == email)
                .Single();

            return result!;
        }

        public static async Task UpdateAuthStatus(int id, bool auth)
        {
            var value = await _supabase!
                    .From<Users>()
                    .Where(x => x.Id == id)
                    .Set(x => x.AuthenticationStatus, auth)
                    .Update();
        }

        public static async Task<Users> GetUserByEmail(string email)
        {
            var result = await _supabase!
                .From<Users>()
                .Select(x => (new object[] { x.Id, x.Avatar!, x.LastLogin!, x.Email!, x.Username! }))
                .Where(x => x.Email == email)
                .Single();

            return result!;
        }

        public static async Task<Users> GetUserByUsername(string Username)
        {
            var result = await _supabase!.From<Users>().Select(x => new object[] { x.Id, x.Avatar!, x.LastLogin!, x.Email!, x.Username! }).Where(x => x.Username == Username).Single();

            return result!;
        }

        public static async Task<Postgrest.Responses.ModeledResponse<UserChatUsers>> CreateChatConnection(List<UserChatUsers> models)
        {
            return await _supabase!.From<UserChatUsers>().Insert(models);
        }

        public static async Task<Users> GetUserById(int id)
        {
            var result = await _supabase!
                .From<Users>()
                .Select(x => (new object[] { x.Id, x.Avatar!, x.LastLogin!, x.Email!, x.Username! }))
                .Where(x => x.Id == id)
                .Single();

            return result!;
        }

        public static async Task<string> GetChatsByUserId(int Id)
        {
            var userChatIdList = JsonConvert.DeserializeObject<List<UserChatUsers>>((await _supabase!
                .From<UserChatUsers>()
                .Select(x => (new object[] { x.UserChatId }))
                .Where(x => x.UserId == Id)
                .Get()).Content!)!.Select(x => x.UserChatId).Cast<object>().ToList();

            var result = await _supabase
                .From<UserChatUsers>()
                .Select("Users:UserId(Id, Username, Avatar, LastLogin, Email), UserChats:UserChatId(Id, ChatType, ChatName, Avatar), Messages:LastMessage(Message, Users:SenderId(Username), Time)")
                .Filter(x => x.UserChatId, Operator.In, userChatIdList)
                .Where(x => x.UserId != Id)
                .Order("Messages", "Time", Ordering.Descending)
                .Get();

            return result.Content!;
        }

        public static async Task SetNewLastLoginById(int userId, DateTimeOffset time)
        {
            var value = await _supabase!
                    .From<Users>()
                    .Where(x => x.Id == userId)
                    .Set(x => x.LastLogin!, time)
                    .Update();
        }

        public static async Task<string> GetContactsIdsByChatId(int chatId, int userId)
        {
            var chat = await _supabase!
                .From<UserChatUsers>()
                .Select("Users:UserId(Id)")
                .Where(x => x.UserChatId == chatId && x.UserId != userId)
                .Get();

            return chat!.Content!;
        }

        public static async Task<Users> GetUserByEmailAndPassword(string email, string password)
        {
            var result = await _supabase!
                .From<Users>()
                .Select(x => new object[] { x.Id, x.Username!, x.Email!, x.AuthenticationStatus, x.Avatar! })
                .Where(x =>
                        x.Email == email &&
                        x.Password == password
                        )
                .Single();

            return result!;
        }

        public static async Task<Users> InsertUserToTableUsers(Users user)
        {
            return (await _supabase!.From<Users>().Select(x => new object[] { x.Id, x.Username!, x.Email!, x.AuthenticationStatus }).Insert(user!)).Model!;
        }

        public static async Task<HttpResponseMessage> UpdateAuthByEmail(string email, bool auth)
        {
            var value = await _supabase!
                .From<Users>()
                .Where(x => x.Email == email)
                .Set(x => x.AuthenticationStatus, auth)
                .Update();

            return value.ResponseMessage!;
        }

        public static async Task InsertMessageToTableMessages(Messages message)
        {
            _ = await _supabase!.From<Messages>().Insert(message!);
        }

        public static async Task<HttpResponseMessage> UpdatePassword(Users myObject)
        {
            var value = await _supabase!
                        .From<Users>()
                        .Where(x => x.Email == myObject.Email)
                        .Set(x => x.AuthenticationStatus, false)
                        .Set(x => x.Password!, myObject.Password)
                        .Update();

            return value.ResponseMessage!;
        }

        public static async Task<string> GetMessagesByChatId(int chatId)
        {
            var messages = await _supabase!
                    .From<Messages>()
                    .Select("Users:SenderId(Username), Time, Message")
                    .Where(x => x.UserChatId == chatId)
                    .Get();

            return messages.Content!;
        }

        public static async Task<Postgrest.Responses.ModeledResponse<UserChats>> CreateNewChat()
        {
            return await _supabase!.From<UserChats>().Insert(new UserChats { ChatType = ChatType.Chat });
        }

        public static async Task<Postgrest.Responses.ModeledResponse<Users>> FindUsersByUsername(string Username, int Id)
        {
            return await _supabase!.From<Users>()
                  .Filter(x => x.Username!, Operator.ILike, Username + "%")
                  .Where(x => x.Id != Id)
                  .Limit(5)
                  .Get();
        }
    }
}
