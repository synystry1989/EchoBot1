using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EchoBot1.Modelos
{
    // A classe ChatContext herda de CommonEntity
    // Verifique se CommonEntity já implementa ITableEntity
    public class ChatContext : CommonEntity
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("messages")]
        public List<Message> Messages { get; set; } = new List<Message>();

        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        // Construtor padrão
        public ChatContext() { }

        // Construtor para inicializar as chaves e outros campos
        public ChatContext(string userId, string conversationId)
        {
            PartitionKey = userId ?? throw new ArgumentNullException(nameof(userId));
            RowKey = conversationId ?? throw new ArgumentNullException(nameof(conversationId));
            ConversationId = conversationId;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public class Message
        {
            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }
        }
    }
}
