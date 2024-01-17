namespace Server.Tables
{
    internal enum ChatType
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
        ChatType? Type { get; set; }
        public string? ChatName { get; }
        string? LastMessage { get; set; }
    }

    internal class ChatModel : IChatGroupModels
    {
        public int? ChatId { get; set; }
        public string? ChatName { get; set; }
        public ContactModel? Contact { get; set; }
        public ChatType? Type { get; set; } = ChatType.CHAT;
        public string? LastMessage { get; set; } = string.Empty;

        public ChatModel(ChatData data)
        {
            ChatId = data.UserChats!.Id;
            ChatName = data.Users!.Username ?? string.Empty;
            Contact = new ContactModel(data.Users);
            Type = (ChatType)data.UserChats.ChatType;
            LastMessage = data.Messages?.Message ?? string.Empty;
        }
        public ChatModel() {}
    }

    internal class GroupModel : IChatGroupModels
    {
        public int? ChatId { get; set; }
        public string? ChatName { get; set; }
        public List<ContactModel>? ContactsInGroup { get; set; }
        public ChatType? Type { get; set; } = ChatType.GROUP;
        public string? Avatar { get; set; } = string.Empty;
        public string? LastMessage { get; set; } = string.Empty;

        public GroupModel(ChatData data)
        {
            ChatId = data.UserChats!.Id;
            ChatName = data.UserChats.ChatName ?? string.Empty;
            Type = (ChatType)data.UserChats.ChatType;
            Avatar = data.UserChats.Avatar ?? string.Empty;
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
                if (data.UserChats!.ChatType == ChatType.CHAT)
                {
                    chatGroupModels.Add(new ChatModel(data));
                }
                else if (data.UserChats.ChatType == ChatType.GROUP && !chatGroupModels.Any(x => x.ChatId == data.UserChats.Id))
                {
                    chatGroupModels.Add(new GroupModel(data));
                }
            }

            return chatGroupModels;
        }
    }
}
