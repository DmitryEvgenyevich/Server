using NAudio.Midi;
using Newtonsoft.Json;
using Server.Tables;
using Supabase;
using System.Reflection;
using static Postgrest.Constants;

namespace Server.Database
{
    static class Database
    {
        private static SupabaseOptions options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        private static Client? _database;

        public static async Task DatabaseInit()
        {
            _database = new Client(Environment.GetEnvironmentVariable("SUPABASE_URL")!, Environment.GetEnvironmentVariable("SUPABASE_KEY"), options);
            await _database.InitializeAsync();
        }

        //async static public Task SetLastMessage(int chatId, int messageId, int senderId)
        //{
        //    var usersChats = await _database!
        //        .From<UsersChats>()
        //        .Where(y => y.ChatId == chatId)
        //        .Set(x => x.LastMessage!, messageId)
        //        .Update();
        //}

        //async static public Task<Users> GetUserIdByEmail(string email)
        //{
        //    var result = await _database!
        //        .From<Users>()
        //        .Select(y => (new object[] { y.Id }))
        //        .Where(y => y.Email == email)
        //        .Single();

        //    return result!;
        //}

        public static async Task<Users> GetUserByDeviseToken(string token)
        {
            var devise = await _database!
                .From<Devises>()
                .Where(x => x.token == token)
                .Single();

            var user = await _database!
                .From<Users>()
                .Where(x => x.Id == devise!.user_id)
                .Single();

            return user!;
        }

        public static async Task<object> AddNewDevise(Devises devise)
        {
            return await _database!
                .From<Devises>()
                .Insert(devise!);
        }

        ////public static async Task<Users> GetUserByEmail(string email)
        ////{
        ////    var result = await _database!
        ////        .From<Users>()
        ////        .Select(x => (new object[] { x.Id, x.Avatar!, x.LastLogin!, x.Email!, x.Username! }))
        ////        .Where(x => x.Email == email)
        ////        .Single();

        ////    return result!;
        ////}

        ////public static async Task<Users> GetUserByUsername(string Username)
        ////{
        ////    var result = await _database!.From<Users>().Select(x => new object[] { x.Id, x.Avatar!, x.LastLogin!, x.Email!, x.Username! }).Where(x => x.Username == Username).Single();

        ////    return result!;
        ////}

        //public static async Task<Postgrest.Responses.ModeledResponse<UsersChats>> CreateChatConnection(List<UsersChats> models)
        //{
        //    return await _database!.From<UsersChats>().Insert(models);
        //}

        //public static async Task<Users> GetUserById(int id)
        //{
        //    var result = await _database!
        //        .From<Users>()
        //        .Select(x => (new object[] { x.Id, x.Avatar!, x.LastLogin!, x.Email!, x.Username! }))
        //        .Where(x => x.Id == id)
        //        .Single();

        //    return result!;
        //}

        public static async Task<string> GetChatsByUserId(int Id)
        {
            return (await _database!.Rpc("get_chat_data", new Dictionary<string, object> { { "id_of_user", Id } })).Content!;
        }

        public static async Task SetNewLastLoginById(int userId, DateTimeOffset time)
        {
            var value = await _database!
                    .From<Users>()
                    .Where(x => x.Id == userId)
                    .Set(x => x.LastLogin!, time)
                    .Update();
        }

        public static async Task<string> GetContactsIdsByUserChatId(int userChatId, int userId)
        {
            var tenmp = (await _database!.Rpc("get_users_in_same_chat", new Dictionary<string, object> { { "userid", userId }, { "userchatid", userChatId } })).Content!;
            return tenmp;
        }

        public static async Task<Users> GetUserByEmailAndPassword(string email, string password)
        {
            var result = await _database!
                .From<Users>()
                .Select(x => new object[] { x.Id, x.Username!, x.Email! })
                .Where(x =>
                        x.Email == email &&
                        x.Password == password
                        )
                .Single();

            return result!;
        }

        public static async Task<Users> InsertUserToTableUsers(Users user)
        {
            var new_user = await _database!.From<Users>()
                            .Select(x => new object[] { x.Id, x.Username!, x.Email! })
                            .Insert(user!);
            
            return new_user.Model!;
        }

        //public static async Task<HttpResponseMessage> UpdateAuthByEmail(string email, bool auth)
        //{
        //    var value = await _database!
        //        .From<Users>()
        //        .Where(x => x.Email == email)
        //        .Set(x => x.AuthenticationStatus, auth)
        //        .Update();

        //    return value.ResponseMessage!;
        //}

        //async static public Task<Postgrest.Responses.ModeledResponse<Messages>> InsertMessageToTableMessages(Messages message)
        //{
        //    return await _database!.From<Messages>().Select(x => new object[] { x.Id }).Insert(message!);
        //}

        //public static async Task<HttpResponseMessage> UpdatePassword(Users myObject)
        //{
        //    var value = await _database!
        //                .From<Users>()
        //                .Where(x => x.Email == myObject.Email)
        //                .Set(x => x.AuthenticationStatus, false)
        //                .Set(x => x.Password!, myObject.Password)
        //                .Update();

        //    return value.ResponseMessage!;
        //}

        public static async Task<string> GetMessagesByChatId(int chatId)
        {
            return (await _database!.Rpc("get_messages_and_read_status", new Dictionary<string, object> { { "chat_user_id", chatId } })).Content!;
        }

        //public static async Task<Postgrest.Responses.ModeledResponse<Chats>> CreateNewChat()
        //{
        //    return await _database!.From<Chats>().Insert(new Chats { ChatType = TypesOfChat.CHAT });
        //}

        //public static async Task<Postgrest.Responses.ModeledResponse<Users>> FindUsersByUsername(string Username, int Id)
        //{
        //    return await _database!.From<Users>()
        //          .Filter(x => x.Username!, Operator.ILike, Username + "%")
        //          .Where(x => x.Id != Id)
        //          .Limit(5)
        //          .Get();
        //}
    }
}
