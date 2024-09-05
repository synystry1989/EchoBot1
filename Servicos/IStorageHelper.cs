using Azure.Data.Tables;
using EchoBot1.Modelos;
using Microsoft.Bot.Builder;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot1.Servicos
{
    public interface IStorageHelper
    {
        Task SaveUserDataAsync(ITurnContext turnContext, string name, string email, CancellationToken cancellationToken);

        Task<List<string>> GetPaginatedUserIdsAsync();

        Task InsertEntityAsync<T>(string tableName, T entity) where T : ITableEntity;


        Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();

        Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey);

        Task<TableClient> GetTableClient(string tableName);

        //Task InsertChatContextAsync(string userId, string conversationId, string chatContext);

        Task<bool> UserExistsAsync(string userId);

        Task CreateTablesIfNotExistsAsync();

        Task SaveChatContextToStorageAsync(string tableName, string userId, string conversationId, ChatContext chatContext);

        Task<ChatContext> InitializeChatContextAsync(string userId);

        Task<List<string>> GetPaginatedConversationIdsByUserIdAsync(string userId);
    }
}
