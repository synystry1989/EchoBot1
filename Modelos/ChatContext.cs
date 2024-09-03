using Newtonsoft.Json;
using System.Collections.Generic;


namespace EchoBot1.Modelos
{
    public class ChatContext : CommonEntity
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("messages")]

        public List<Message> Messages { get; set; }


        public ChatContext() { }

        public ChatContext(string userId, string conversationId) : base(userId, conversationId)
        {
        }
    }

    public class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }


    }
}
