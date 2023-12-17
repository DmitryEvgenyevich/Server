using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Server.Message;
using Server.Tables;
using Supabase;
using Supabase.Gotrue;
using Supabase.Interfaces;
using static Postgrest.Constants;

namespace Server.Tables
{
    static class DB
    {
        static string supabaseUrl = "https://tfggopkviuvatvbssgev.supabase.co";

        static string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InRmZ2dvcGt2aXV2YXR2YnNzZ2V2Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MDAyNDc4ODgsImV4cCI6MjAxNTgyMzg4OH0.y7CsjBe31W072cYdZv6SgveBjVD4MrsAINLc8L2PZDI";

        static SupabaseOptions options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        static Supabase.Client supabase;

        public async static Task DBinit()
        {
            supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await supabase.InitializeAsync();
        }

        async static public Task<Users> GetUserIdByEmail(string email)
        {
            var result = await supabase
                .From<Users>()
                .Select(y => (new object[] { y.Id }))
                .Where(y => y.Email == email)
                .Single();

            return result!;
        }

        async static public Task<HttpResponseMessage> UpdateAuthStatus(string email, bool auth)
        {
            var value = await supabase
                    .From<Tables.Users>()
                    .Where(x => x.Email == email)
                    .Set(x => x.Auth, auth)
                    .Update();

            return value.ResponseMessage!;
        }

        async static public Task<Users> GetUserByEmail(string email)
        {
            var result = await supabase
                .From<Users>()
                .Select(x => (new object[] { x.Id, x.Avatar, x.LastLogin, x.Email, x.Username }))
                .Where(x => x.Email == email)
                .Single();

            return result!;
        }

        async static public Task<Users> GetUserByUsername(string Username)
        {
            var result = await supabase.From<Users>().Select(x => new object[] { x.Id, x.Avatar, x.LastLogin, x.Email, x.Username }).Where(x => x.Username == Username).Single();

            return result!;
        }

        async static public Task<Postgrest.Responses.ModeledResponse<UserChatUsers>> CreateChatConnection(List<UserChatUsers> models)
        {
            return await supabase.From<UserChatUsers>().Insert(models);
        }
        
        async static public Task<Users> GetUserById(int id)
        {
            var result = await supabase
                .From<Users>()
                .Select(x => (new object[] { x.Id, x.Avatar, x.LastLogin, x.Email, x.Username }))
                .Where(x => x.Id == id)
                .Single();

            return result!;
        }

        async static public Task<HttpResponseMessage> UpdateAuthCodeByEmail(string email, int auth_code)
        {
            var value = await supabase
                .From<Users>()
                .Where(x => x.Email == email)
                .Set(x => x.AuthCode, auth_code)
                .Update();

            return value.ResponseMessage!;
        }

        async static public Task<List<UserChatUsers>> GetChatsByUserId(int Id)
        {
            var chats = await supabase
                .From<UserChatUsers>()
                .Select(x => new object[] { x.UserChatId })
                .Where(x => x.UserId == Id)
                .Get();

            return chats!.Models;
        }

        async static public Task<string> GetContactEmailByChatId(int chatId, int userId)
        {
            var chat = await supabase
                .From<UserChatUsers>()
                .Select("Users:UserId(Id, Username, Avatar, LastLogin, Email), UserChats:UserChatId(LastMessage, ChatType, ChatName, Avatar)")
                .Where(x => x.UserChatId == chatId && x.UserId != userId)
                .Get();

            return chat!.Content;
        }

        async static public Task SetNewLastLoginById(int userId, DateTimeOffset time)
        {
            var value = await supabase
                    .From<Tables.Users>()
                    .Where(x => x.Id == userId)
                    .Set(x => x.LastLogin, time)
                    .Update();
        }
        async static public Task<string> GetContactsIdsByChatId(int chatId, int userId)
        {
            var chat = await supabase
                .From<UserChatUsers>()
                .Select("Users:UserId(Id)")
                .Where(x => x.UserChatId == chatId && x.UserId != userId)
                .Get();

            return chat!.Content;
        }

        async static public Task<Users> GetUserByEmailAndPassword(string email, string password)
        {
            var result = await supabase
                .From<Users>()
                .Select(x => new object[] { x.Id, x.Username, x.Email, x.Auth, x.Avatar })
                .Where(x =>
                        x.Email == email &&
                        x.Password == password
                        )
                .Single();

            return result!;
        }

        async static public Task<HttpResponseMessage> InsertUserToTableUsers(Users user)
        {
            return (await supabase.From<Users>().Insert(user!)).ResponseMessage!;
        }
        
        async static public Task<HttpResponseMessage> UpdateAuthByEmail(string email, bool auth)
        {
            var value = await supabase
                .From<Users>()
                .Where(x => x.Email == email)
                .Set(x => x.Auth, auth)
                .Update();

            return value.ResponseMessage!;
        }

        async static public Task InsertMessageToTableMessages(Messages message)
        {
            _ = await supabase.From<Messages>().Insert(message!);
        }

        async static public Task<HttpResponseMessage> UpdatePassword(Users myObject)
        {
            var value = await supabase
                        .From<Tables.Users>()
                        .Where(x => x.Email == myObject.Email)
                        .Set(x => x.Auth, myObject.Auth)
                        .Set(x => x.Password, myObject.Password)
                        .Update();

            return value.ResponseMessage!;
        }

        async static public Task<string> GetMessages(Messages myObject)
        {
            var messages = await supabase
                    .From<Messages>()
                    .Select("Users:SenderId(Username), Time, Message")
                    .Where(x => x.UserChatId == myObject.UserChatId)
                    .Get();

            return messages.Content;
        }

        async static public Task<Postgrest.Responses.ModeledResponse<UserChats>> CreateNewChat()
        {
            return await supabase.From<UserChats>().Insert(new UserChats { ChatType = ChatType.Chat });
        }

        async static public Task<Postgrest.Responses.ModeledResponse<Users>> FindUsersByUsername(Users data)
        {
            return await supabase.From<Users>()
                  .Filter(x => x.Username, Operator.ILike, data.Username + "%")
                  .Where(x => x.Id != data.Id)
                  .Limit(5)
                  .Get();
        }
    }   
}
