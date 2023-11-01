using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Server.Message;
using Server.Tables;
using Supabase;
using Supabase.Gotrue;
using Supabase.Interfaces;

namespace Server.Tables
{
    static class DB
    {
        static string supabaseUrl = "https://xqtbulboyjkpozsnyttc.supabase.co";

        static string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InhxdGJ1bGJveWprcG96c255dHRjIiwicm9sZSI6ImFub24iLCJpYXQiOjE2OTIxMTM1MzcsImV4cCI6MjAwNzY4OTUzN30.qPD7zjTMELHmJV7Tkynn8WwyLFmh2uO0-_tU3EQk_H0";

        static SupabaseOptions options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        async static Task<Supabase.Client> _getSupabaseClient()
        {
            var supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);
            await supabase.InitializeAsync();

            return supabase;
        }

        async static public Task<Users> GetUserIdByEmail(string email)
        {
            var supabase = await _getSupabaseClient();

            var result = await supabase
                .From<Users>()
                .Select(y => (new object[] { y.Id }))
                .Where(y => y.Email == email)
                .Single();

            return result!;
        }

        async static public Task<Users> GetUserById(int id)
        {
            var supabase = await _getSupabaseClient();

            var result = await supabase
                .From<Users>()
                .Select(y => (new object[] { y.Id, y.Avatar, y.Username }))
                .Where(y => y.Id == id)
                .Single();

            return result!;
        }

        async static public Task<HttpResponseMessage> UpdateAuthCodeByEmail(string email, int auth_code)
        {
            var supabase = await _getSupabaseClient();

            var value = await supabase
                .From<Users>()
                .Where(x => x.Email == email)
                .Set(x => x.Auth_code, auth_code)
                .Update();

            return value.ResponseMessage!;
        }

        async static public Task<List<UserChatUsers>> GetChatsByUserId(int Id)
        {
            var supabase = await _getSupabaseClient();

            var chats = await supabase
                .From<UserChatUsers>()
                .Select(x => new object[] { x.user_chat_id })
                .Where(x => x.user_id == Id)
                .Get();

            return chats!.Models;
        }

        async static public Task<int> GetContactEmailByChatId(int chatId, int userId)
        {
            var supabase = await _getSupabaseClient();

            var chat = await supabase
                .From<UserChatUsers>()
                .Select(x => new object[] { x.user_id })
                .Where(x => x.user_chat_id == chatId && x.user_id != userId)
                .Single();

            return chat.user_id;

        }

        async static public Task<Users> GetUserByEmailAndPasswordIsRight(string email, string password)
        {
            var supabase = await _getSupabaseClient();

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
            var supabase = await _getSupabaseClient();

            return (await supabase.From<Users>().Insert(user!)).ResponseMessage!;
        }
        
        async static public Task<HttpResponseMessage> UpdateAuthByEmail(string email, bool auth)
        {
            var supabase = await _getSupabaseClient();

            var value = await supabase
                .From<Users>()
                .Where(x => x.Email == email)
                .Set(x => x.Auth, auth)
                .Update();

            return value.ResponseMessage!;
        }

        async static public Task<HttpResponseMessage> InsertMessageToTableMessages(Messages message)
        {
            var supabase = await _getSupabaseClient();

            return (await supabase.From<Messages>().Insert(message!)).ResponseMessage!;
        }
    }   
}
