using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Concurrent;

namespace EchoBot1.Modelos
{
    public class CommonEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; } = default;
        public ETag ETag { get; set; } = default;



        public CommonEntity() { }

        public CommonEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey ?? throw new ArgumentNullException(nameof(partitionKey));
            RowKey = rowKey ?? throw new ArgumentNullException(nameof(rowKey));
        }
    }
}
