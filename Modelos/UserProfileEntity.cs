using Azure;
using Azure.Data.Tables;
using System;

namespace EchoBot1.Modelos
{
    public class UserProfileEntity : CommonEntity
    {
  
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Morada { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string NIF { get; set; } = string.Empty;

        public UserProfileEntity() { }

        public UserProfileEntity(string userId, string conversationId)
        {
            PartitionKey = userId ?? throw new ArgumentNullException(nameof(userId));
            RowKey = conversationId ?? throw new ArgumentNullException(nameof(conversationId)); ;
        }
    }
}
