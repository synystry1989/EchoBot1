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
 
        public async Task CreateTablesIfNotExistsAsync()
        {
            // Create UserContext table
            var userContextTableName = _configuration["StorageAcc:GPTContextTable"];
            await GetTableClient(userContextTableName);

            // Create UserProfiles table
            var userProfileTableName = _configuration["StorageAcc:UserProfileTable"];
            await GetTableClient(userProfileTableName);
        }

        public async Task InsertEntityAsync<T>(string tableName, T entity) where T : ITableEntity
        {
            try
            {
                var tableClient = await GetTableClient(tableName);
                await tableClient.UpsertEntityAsync(entity);
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
                var tableClient = await GetTableClient(tableName);
                return await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Entity not found
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting entity from table storage: {ex.Message}");
            }
        }

        public async Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey)
        {
            try
            {
                var tableClient = await GetTableClient(tableName);
                await tableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting entity from table storage: {ex.Message}");
            }
        }
        public async Task<TableClient> GetTableClient(string tableName)
        {
            TableServiceClient tableServiceClient = new TableServiceClient(_configuration.GetConnectionString("StorageAcc"));
            TableClient tableClient = tableServiceClient.GetTableClient(tableName: tableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

     
   
        public async Task SaveChatContextToStorageAsync(string tableName, string userId, string conversationId, ChatContext chatContext)
        {
            var chatContextEntity = new GptResponseEntity()
            {
                PartitionKey = userId,
                RowKey = conversationId,
                ConversationId = conversationId,
                UserId = userId,
                UserContext = JsonConvert.SerializeObject(chatContext.Messages)
            };

            await InsertEntityAsync(tableName, chatContextEntity);
        }

       //public async Task InsertChatContextAsync(string userId, string conversationId, string chatContext)
       //  {
       //     var entity = new GptResponseEntity(userId, conversationId,chatContext);
       //     await InsertEntityAsync("UserContext", entity);
       // }
        public async Task<bool> UserExistsAsync(string userId)
        {
            try
            {
                var tableClient = await GetTableClient("UserProfiles");
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
        public async Task<ChatContext> InitializeChatContextAsync(string userId)
        {
            bool userExists = await UserExistsAsync(userId);
            ChatContext chatContext = new ChatContext
            {
                Model = _configuration["OpenAI:Model"],
                Messages = new List<Message>()
            };

            if (userExists)
            {
                var conversationIds = await GetPaginatedConversationIdsByUserIdAsync(userId);

                foreach (var conversationId in conversationIds)
                {
                    var chatContextEntity = await GetEntityAsync<GptResponseEntity>(_configuration["StorageAcc:GPTContextTable"], userId, conversationId);
                    if (chatContextEntity != null)
                    {
                        var previousMessages = JsonConvert.DeserializeObject<List<Message>>(chatContextEntity.UserContext);
                        chatContext.Messages.AddRange(previousMessages);
                    }
                }
            }
            else
            {

                chatContext = new ChatContext()
                {
                    Model = _configuration["OpenAI:Model"],
                    Messages = new List<Message>()
                };
            }

            return chatContext;
        }

        //funcao igual a GetPaginatedConversationIdsByUserIdAsync mas para obter os ids dos users na tabela userprofiles
        public async Task<List<string>> GetPaginatedUserIdsAsync()
        {
            var userIds = new List<string>();
            var tableClient = await GetTableClient("UserProfiles");

            var query = tableClient.QueryAsync<TableEntity>();

            await foreach (var entity in query)
            {
                userIds.Add(entity.PartitionKey);
            }

            return userIds;
        }




        public async Task<List<string>> GetPaginatedConversationIdsByUserIdAsync(string userId)
        {
            var conversationIds = new List<string>();
            var tableClient = await GetTableClient("UserContext");

            var query = tableClient.QueryAsync<GptResponseEntity>(filter: $"PartitionKey eq '{userId}'");

            await foreach (var entity in query)
            {
                conversationIds.Add(entity.RowKey);
            }

            return conversationIds;
        }

    }
}
