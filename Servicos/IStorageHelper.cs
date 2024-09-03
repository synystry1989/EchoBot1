using Azure.Data.Tables;
using EchoBot1.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EchoBot1.Servicos
{
    public interface IStorageHelper
    {
        /// <summary>
        /// Inserts or updates an entity in the specified table.
        /// </summary>
        /// <typeparam name="T">The type of the entity to insert or update.</typeparam>
        /// <param name="tableName">The name of the table where the entity will be stored.</param>
        /// <param name="entity">The entity to insert or update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertEntityAsync<T>(string tableName, T entity) where T : ITableEntity;

        /// <summary>
        /// Retrieves an entity from the specified table by its partition key and row key.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="tableName">The name of the table from which to retrieve the entity.</param>
        /// <param name="partitionKey">The partition key of the entity to retrieve.</param>
        /// <param name="rowKey">The row key of the entity to retrieve.</param>
        /// <returns>The entity retrieved from the table, or null if not found.</returns>
        Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();

        /// <summary>
        /// Deletes an entity from the specified table by its partition key and row key.
        /// </summary>
        /// <param name="tableName">The name of the table from which to delete the entity.</param>
        /// <param name="partitionKey">The partition key of the entity to delete.</param>
        /// <param name="rowKey">The row key of the entity to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey);

        /// <summary>
        /// Retrieves all conversation IDs associated with a specific userId from the UserContext table.
        /// </summary>
        /// <param name="userId">The userId for which to retrieve conversation IDs.</param>
        /// <returns>A list of conversation IDs associated with the specified userId.</returns>
        Task<List<string>> GetConversationIdsByUserIdAsync(string userId);

        /// <summary>
        /// Gets the TableClient for a specified table.
        /// </summary>
        /// <param name="tableName">The name of the table for which to get the TableClient.</param>
        /// <returns>A task representing the asynchronous operation, with the TableClient as the result.</returns>
        Task<TableClient> GetTableClient(string tableName);

        /// <summary>
        /// Inserts or updates a ChatContext in the UserContext table.
        /// </summary>
        /// <param name="userId">The userId associated with the ChatContext.</param>
        /// <param name="conversationId">The conversationId associated with the ChatContext.</param>
        /// <param name="chatContext">The ChatContext object to insert or update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertChatContextAsync(string userId, string conversationId, string chatContext);

        Task<bool> UserExistsAsync(string userId);
    }
}
