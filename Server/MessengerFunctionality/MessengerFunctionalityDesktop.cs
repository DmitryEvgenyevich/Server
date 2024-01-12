﻿using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Server.Message;
using Server.Tables;
using System.Net.Sockets;

namespace Server.MessengerFunctionality
{
    public class MessengerFunctionalityDesktop : IMessengerFunctionality
    {
        public async Task<Response> SignIn(TcpClient clientSocket, string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Users>(json);

                var result = await Database.Database.GetUserByEmailAndPassword(user?.Email!, user?.Password!);

                if (result?.Id == null)
                    return new Response { ErrorMessage = "We can not to find this user, Email or password is not right" };

                if (result.Auth)
                   _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(result.Id, clientSocket);

                return new Response { Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = ex.Message };
            }
        }

        public async Task<Response> SignUp(TcpClient clientSocket, string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Users>(json);

                HttpResponseMessage httpResponse = await Database.Database.InsertUserToTableUsers(user!);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtilities.GlobalUtilities.GetErrorMessage(ex);
            }
        }

        public async Task<Response> GetMyContacts(TcpClient clientSocket, string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Users>(json);

                _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(user!.Id, clientSocket);

                var chatData = System.Text.Json.JsonSerializer.Deserialize<List<ChatData>>(await Database.Database.GetChatsByUserId(user!.Id));

                return new Response { Data = JsonConvert.SerializeObject(ChatDataToIChatModels.Convert(chatData!)) };
            }
            catch (Exception ex)
            {
                return GlobalUtilities.GlobalUtilities.GetErrorMessage(ex);
            }
        }

        public Response SendNewCode(TcpClient clientSocket, string json)
        {
            try
            {
                Users user = JsonConvert.DeserializeObject<Users>(json)!;

                _ = Database.Database.UpdateAuthCodeBysersEmail(user.Email!, user.AuthCode);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtilities.GlobalUtilities.GetErrorMessage(ex);
            }
        }

        public async Task<Response> AuthSuccess(TcpClient clientSocket, string json)
        {
            try
            {
                var myObject = JsonConvert.DeserializeObject<Users>(json)!;

                var user = await Database.Database.UpdateAuthStatus(myObject.Email!, myObject.Auth);

                _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(user.Id, clientSocket);

                return new Response { Data = JsonConvert.SerializeObject(user) };
            }
            catch (Exception ex)
            {
                return GlobalUtilities.GlobalUtilities.GetErrorMessage(ex);
            }
        }

        public async Task<Response> ForgotPassword(TcpClient clientSocket, string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Users>(json)!;

                await Database.Database.UpdatePassword(user);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtilities.GlobalUtilities.GetErrorMessage(ex);
            }
        }

        public async Task<Response> SendMessageInGroup(TcpClient clientSocket, string json)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<Messages>(json)!;

                _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(message.SenderId, clientSocket);

                var usersList = JsonConvert.DeserializeObject<List<Wrapper>>(await Database.Database.GetContactsIdsByChatId(message.UserChatId, message.SenderId))!
                                            .ConvertAll(wrapper => wrapper.Users);

                _ = Users.TryToSendToUsers(usersList!, message, (await Database.Database.GetUserById(message.SenderId)).Username!);

                _ = Database.Database.InsertMessageToTableMessages(message);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtilities.GlobalUtilities.GetErrorMessage(ex);
            }
        }

        public async Task<Response> SendMessageInChat(TcpClient clientSocket, string json)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<Messages>(json)!;

                _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(message.SenderId, clientSocket);

                _ = Users.TryToSendToUser(JObject.Parse(json).Value<int>("RecipientId"), message, (await Database.Database.GetUserById(message.SenderId)).Username!);

                _ = Database.Database.InsertMessageToTableMessages(message);
                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtilities.GlobalUtilities.GetErrorMessage(ex);
            }
        }

        public async Task<Response> GetMessagesByContact(TcpClient clientSocket, string json)
        {
            try
            {
                _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(JObject.Parse(json).Value<int>("UserId"), clientSocket);

                List<ChatMessages> chatMessages = new List<ChatMessages>();

                var jsonArray = JArray.Parse(await Database.Database.GetMessagesByChatId(JObject.Parse(json).Value<int>("UserChatId")));

                foreach (var item in jsonArray)
                {
                    chatMessages.Add(new ChatMessages { Message = item["Message"]!.ToString(), Username = item["Users"]!["Username"]!.ToString(), Time = DateTimeOffset.Parse(item["Time"]!.ToString()) });
                }

                return new Response { Data = JsonConvert.SerializeObject(chatMessages) };
            }
            catch (Exception ex)
            {
                return GlobalUtilities.GlobalUtilities.GetErrorMessage(ex);
            }
        }

        public Response logOut(TcpClient clientSocket, string json)
        {
            try
            {
                _ = OnlineUsers.OnlineUsers.DeleteUserFromOnlineList(clientSocket);
                Console.WriteLine("Client disconnected.");

                return new Response { };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Response { ErrorMessage = ex.Message };
            }
        }

        public async Task<Response> CreateNewChat(TcpClient clientSocket, string json)
        {
            try
            {
                var dataForNewChat = JsonConvert.DeserializeObject<NewChat>(json);

                _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(dataForNewChat!.SenderId, clientSocket);
                
                var newChat = await Database.Database.CreateNewChat();

                var recipient = await Database.Database.GetUserByUsername(dataForNewChat.RecipientUsername!);

                _ = Database.Database.CreateChatConnection(new List<UserChatUsers>
                    {
                        new UserChatUsers{UserChatId = (int)newChat.Model!.Id, UserId = dataForNewChat.SenderId},
                        new UserChatUsers{UserChatId = (int)newChat.Model.Id, UserId = recipient.Id}
                    }
                );

                _ = Users.TryToSendNotification(recipient.Id, (int)newChat.Model.Id, (await Database.Database.GetUserById(dataForNewChat.SenderId)).Username!);

                return new Response { Data = JsonConvert.SerializeObject(new ChatModel
                {
                    ChatId = (int)newChat.Model.Id,
                    Contact = new ContactModel(recipient),
                    ChatName = recipient.Username,
                    Type = ChatType.Chat
                }) };
            }
            catch (Exception ex)
            {
                return GlobalUtilities.GlobalUtilities.GetErrorMessage(ex);
            }
        }

        public async Task<Response> FindUserByUsername(TcpClient clientSocket, string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Users>(json);

                _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(user!.Id, clientSocket);

                var users = await Database.Database.FindUsersByUsername(user.Username!, user.Id);

                List<object> findUsers = new List<object>();

                foreach (var item in JArray.Parse(users.Content!))
                {
                    findUsers.Add(new { ChatName = item["Username"]!.ToString() });
                }

                return new Response { Data = JsonConvert.SerializeObject(findUsers) };
            }
            catch (Exception ex)
            {
                return GlobalUtilities.GlobalUtilities.GetErrorMessage(ex);
            }
        }
    }
}
