using Azure;
using Azure.Data.Tables;
using EchoBot1.Modelos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EchoBot1.Servicos
{
    public class StorageHelper : IStorageHelper
    {
        private readonly IConfiguration _configuration;

        public StorageHelper(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<TableClient> GetTableClient(string tableName)
        {
            TableServiceClient tableServiceClient = new TableServiceClient(_configuration.GetConnectionString("StorageAcc"));
            TableClient tableClient = tableServiceClient.GetTableClient(tableName: tableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        public async Task InsertEntityAsync<T>(string tableName, T entity) where T : ITableEntity
        {
            try
            {
                TableClient table = await GetTableClient(tableName);
                await table.UpsertEntityAsync(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting entity to table storage: {ex.Message}");
            }
        }

        public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            try
            {
                TableClient table = await GetTableClient(tableName);
                return await table.GetEntityAsync<T>(partitionKey, rowKey);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Entity not found
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Error getting entity from table storage: {ex.Message}");
                return null;
            }
        }

        public async Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey)
        {
            try
            {
                TableClient table = await GetTableClient(tableName);
                await table.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting entity from table storage: {ex.Message}");
            }
        }

        public async Task<List<string>> GetConversationIdsByUserIdAsync(string userId)
        {
            try
            {
                var tableClient = await GetTableClient("UserContext");
                var query = tableClient.QueryAsync<GptResponseEntity>(filter: $"PartitionKey eq '{userId}'");

                var conversationIds = new List<string>();
                await foreach (var entity in query)
                {
                    conversationIds.Add(entity.RowKey); // Use RowKey to get conversationId
                }

                return conversationIds;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Error retrieving conversation IDs: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task InsertChatContextAsync(string userId, string conversationId, string chatContext)
        {
            var entity = new GptResponseEntity(userId, conversationId,chatContext);
            await InsertEntityAsync("UserContext", entity);
        }
        public async Task<bool> UserExistsAsync(string userId)
        {
            try
            {
                var tableClient = await GetTableClient("UserContext");
                var query = tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{userId}'");

                await foreach (var entity in query)
                {
                    // If we find any entity with the given userId, return true
                    return true;
                }

                // If no entity is found, return false
                return false;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Error checking if user exists: {ex.Message}");
                return false;
            }
        }

    }
}
