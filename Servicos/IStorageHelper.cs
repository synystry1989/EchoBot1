using Azure.Data.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EchoBot1.Servicos
{
    public interface IStorageHelper
    {
        Task InsertEntityAsync<T>(string tableName, T entity) where T : ITableEntity;
        Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();
        Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey);
        Task<IEnumerable<T>> GetEntitiesByPartitionKeyAsync<T>(string tableName, string partitionKey) where T : class, ITableEntity, new();

        // Adicionar novo método para obter conversationId associados a um userId
        Task<IEnumerable<string>> GetConversationIdsForUserAsync(string tableName, string userId);
    }
}
