using System.Collections.Concurrent;

namespace EchoBot1.Modelos
{

    public class GptResponseEntity : CommonEntity
    {
        public string UserContext { get; set; }

        public GptResponseEntity() { }

        public GptResponseEntity(string userId, string responseId)
        {
            PartitionKey = userId;
            RowKey = responseId;
        }
    }
}


