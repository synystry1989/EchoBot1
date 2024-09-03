using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EchoBot1.Modelos
{
    public class GptResponseEntity : CommonEntity
    {
        // PartitionKey will store the userId

        // This property will store the serialized chat context
        public string UserId { get; set; }

        // Property to explicitly store conversationId
        public string ConversationId { get; set; }

        // This property will store the serialized chat context
        public string UserContext { get; set; }

        // Constructors to initialize the entity
        public GptResponseEntity() { }

        public GptResponseEntity(string userId, string conversationId, string userContext)
        {
            PartitionKey = userId;
            RowKey = conversationId;
            UserId = userId;
            ConversationId = conversationId;
            UserContext = userContext;
        }

    }
}
