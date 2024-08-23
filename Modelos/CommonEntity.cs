using Azure;
using Azure.Data.Tables;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Pkcs;
using System;

namespace EchoBot1.Modelos
{
    public class CommonEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;

        public DateTimeOffset? Timestamp { get; set; } = default!;

        public ETag ETag { get; set; } = default;

    }
}
