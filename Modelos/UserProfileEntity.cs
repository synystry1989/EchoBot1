using OpenAI_API.Chat;

namespace EchoBot1.Modelos
{
    public class UserProfileEntity: CommonEntity
    {
        string UserId { get; set; }

        string UserName { get; set; }

        string UserEmail { get; set; }

        string UserPhone { get; set; }

        string UserPassword { get; set; }

        public UserProfileEntity() { }

        public UserProfileEntity(string userId, string userName, string userEmail, string userPhone, string userPassword,string conversationId)
        {
            PartitionKey = userId;
            RowKey = conversationId;
            UserId = userId;
            UserName = userName;
            UserEmail = userEmail;
            UserPhone = userPhone;
            UserPassword = userPassword;
            
        }

    }
}
