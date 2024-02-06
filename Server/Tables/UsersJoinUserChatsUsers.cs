namespace Server.Tables
{
    internal enum TypesOfChat
    {
        CHAT = 1,
        GROUP = 2
    };

    internal class ContactModel
    {
        public int Id;
        public string? Username;
        public string? Email;
        public string? Avatar;
        public DateTimeOffset? LastLogin;

        public ContactModel(Users user)
        {
            Avatar = user.Avatar;
            Email = user.Email;
            Id = user.Id;
            Username = user.Username;
            LastLogin = user.LastLogin;
        }
    }

    internal interface IChatGroupModels
    {
        int? ChatId { get; set; }
        TypesOfChat? Type { get; set; }
        public string? ChatName { get; }
        string? LastMessage { get; set; }
    }

    internal class ChatModel : IChatGroupModels
    {
        public int? ChatId { get; set; }
        public string? ChatName { get; set; }
        public ContactModel? Contact { get; set; }
        public TypesOfChat? Type { get; set; } = TypesOfChat.CHAT;
        public string? LastMessage { get; set; } = string.Empty;

        public ChatModel(ChatData data)
        {
            ChatId = data.Chats!.Id;
            ChatName = data.Users!.Username ?? string.Empty;
            Contact = new ContactModel(data.Users);
            Type = (TypesOfChat)data.Chats.ChatType;
            LastMessage = data.Messages?.Message ?? string.Empty;
        }
        public ChatModel() {}
    }

    internal class GroupModel : IChatGroupModels
    {
        public int? ChatId { get; set; }
        public string? ChatName { get; set; }
        public List<ContactModel>? ContactsInGroup { get; set; }
        public TypesOfChat? Type { get; set; } = TypesOfChat.GROUP;
        public string? Avatar { get; set; } = string.Empty;
        public string? LastMessage { get; set; } = string.Empty;

        public GroupModel(ChatData data)
        {
            ChatId = data.Chats!.Id;
            ChatName = data.Chats.ChatName ?? string.Empty;
            Type = (TypesOfChat)data.Chats.ChatType;
            Avatar = data.Chats.Avatar ?? string.Empty;
            LastMessage = data.Messages?.Message ?? string.Empty;
        }
    }

    internal class ChatDataToIChatModels
    {
        static public List<IChatGroupModels> Convert(List<ChatData> chatData)
        {
            var chatGroupModels = new List<IChatGroupModels>();

            foreach (var data in chatData)
            {
                if (data.Chats!.ChatType == TypesOfChat.CHAT)
                {
                    chatGroupModels.Add(new ChatModel(data));
                }
                else if (data.Chats.ChatType == TypesOfChat.GROUP && !chatGroupModels.Any(x => x.ChatId == data.Chats.Id))
                {
                    chatGroupModels.Add(new GroupModel(data));
                }
            }

            return chatGroupModels;
        }
    }
}
