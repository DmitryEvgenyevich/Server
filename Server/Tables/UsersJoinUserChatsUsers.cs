
namespace Server.Tables
{
    public enum ChatType
    {
        Chat = 1,
        Group = 2
    };

    public class ContactModel
    {
        public int Id;

        public string? Username;

        public string? Email;

        public string? Avatar;

        public DateTimeOffset? LastLogin;
    }

    public interface IChatGroupModels
    {
        int ChatId { get; set; }

        ChatType Type { get; set; }

        public string ChatName { get; }

        string? LastMessage { get; set; }
    }

    public class ChatModel : IChatGroupModels
    {
        public int ChatId { get; set; }

        public string ChatName { get; set; }

        public ContactModel Contact { get; set; }

        public ChatType Type { get; set; } = ChatType.Chat;

        public string? LastMessage { get; set; } = string.Empty;

    }

    public class GroupModel : IChatGroupModels
    {
        public int ChatId { get; set; }

        public string ChatName { get; set; }

        public List<ContactModel> ContactsInGroup { get; set; }

        public ChatType Type { get; set; } = ChatType.Group;

        public string Avatar { get; set; } = string.Empty;

        public string? LastMessage { get; set; } = string.Empty;
    }
}
