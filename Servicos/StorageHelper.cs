using Azure;
using Azure.Data.Tables;
using EchoBot1.Modelos;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;
using System;
using System.Collections.Generic;
using System.Threading;
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
       // guardar com parametros
        public async Task SaveUserDataAsync(string name, string email, string userId)
        {

            await InsertEntityAsync(_configuration["StorageAcc:UserProfileTable"], new PersonalDataEntity()
            {
                PartitionKey = userId,
                RowKey = Guid.NewGuid().ToString(),
                Email = email,
                Name = name
            });
        }
        // create similar to SaveUserDataAsync but not creating new one but completing by userid
        public async Task SaveUserDataAsync(string name, string email, string conversationId, string userId)
        {

            var userProfile = new PersonalDataEntity()
            {
                PartitionKey = userId,
                RowKey = conversationId,
                Name = name,
                Email = email,
                Id = userId

            };
            //List<string> users = await GetPaginatedUserIdsAsync();
            //// Save user data using IStorageHelper

            //foreach (var user in users)
            //{
            //    if (user == userId)
            //    {
            //        userProfile.Email = email;
            //        userProfile.Name = name;
            //    }

                await InsertEntityAsync(_configuration["StorageAcc:UserProfileTable"], userProfile);
            }
        


        //guardar com id
        public async Task InsertUserAsync(string userId, string conversationId)
        {
            var entity = new PersonalDataEntity()
                {PartitionKey = userId,
                RowKey = conversationId,
                Name= "User",
                Email = "",
                Id = userId
            
           };
            await InsertEntityAsync("UserProfiles", entity);
        }
        //funcao para carregar o chat context de um user
        //public async Task<ChatContext> LoadChatContextAsync(string userId, string conversationId)
        //{
        //    var chatContextEntity = await GetEntityAsync<GptResponseEntity>(_configuration["StorageAcc:GPTContextTable"], userId, conversationId);
        //    if (chatContextEntity != null)
        //    {
        //        return new ChatContext()
        //        {
        //            Model = _configuration["OpenAI:Model"],
        //            Messages = JsonConvert.DeserializeObject<List<Message>>(chatContextEntity.UserContext)
        //        };
        //    }
        //    return null;
        //}

        //public async Task SaveChatContextToStorageAsync(string tableName, string userId, string conversationId, ChatContext chatContext)
        //{
        //    var chatContextEntity = new GptResponseEntity()
        //    {
        //        PartitionKey = userId,
        //        RowKey = conversationId,
        //        ConversationId = conversationId,
        //        UserId = userId,
        //        UserContext = JsonConvert.SerializeObject(chatContext.Messages)
        //    };

        //    await InsertEntityAsync(tableName, chatContextEntity);
        //}

     
        //public async Task<bool> UserExistsAsync(string userId)
        //{
        //    try
        //    {
        //        var tableClient = await GetTableClient("UserProfiles");
        //        var query = tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{userId}'");

        //        await foreach (var entity in query)
        //        {
        //            // If we find any entity with the given userId, return true
        //            return true;
        //        }

        //        // If no entity is found, return false
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        await Console.Out.WriteLineAsync($"Error checking if user exists: {ex.Message}");
        //        return false;
        //    }
        //}
        ////adicionar um user a tabela userprofiles
        

        public async Task<string> GetUserNameAsync(string userId,string conversationId)
        {
            try
            {
                var personalData = await GetEntityAsync<PersonalDataEntity>(_configuration["StorageAcc:UserProfileTable"], userId, conversationId);
                return personalData?.Name;
            }
            catch (Exception ex)
            {
         
                return null;
            }
        }
        public async Task InitializeChatContextAsync(string userId,string userMessage,string conversationId)
        {
            
            PersonalDataEntity userProfile = new PersonalDataEntity();
            ChatContext chatContext = null;
            
            if (await GetUserNameAsync(userId,conversationId) != null)

            {
                // Initialize a new chat context to aggregate all messages
                chatContext = new ChatContext()
                {
                    Model = _configuration["OpenAI:Model"],
                    Messages = new List<Message>()
                };

                // Load all previous conversations
                var existingConversationIds = await GetPaginatedConversationIdsByUserIdAsync(userProfile.Id);
                foreach (var existingConversationId in existingConversationIds)
                {
                    var chatContextEntity = await GetEntityAsync<GptResponseEntity>(_configuration["StorageAcc:GPTContextTable"], userProfile.Id, existingConversationId);
                    if (chatContextEntity != null)
                    {
                        // Append each message from the previous context to the current chat context
                        var previousMessages = JsonConvert.DeserializeObject<List<Message>>(chatContextEntity.UserContext);
                        chatContext.Messages.AddRange(previousMessages);
                    }
                }

            }
            else
            // Step 2: Initialize a new chat context if no previous context exists

            {
                chatContext = new ChatContext()
                {
                    Model = _configuration["OpenAI:Model"],
                    Messages = new List<Message>()
                };
            }
            chatContext.Messages.Add(new Message() { Role = "user", Content = userMessage });

            //bool userExists = await UserExistsAsync(userId);
           
            //ChatContext chatContext = null;
            //if (userExists)

            //{
            //     chatContext = new ChatContext
            //    {
            //        Model = _configuration["OpenAI:Model"],
            //        Messages = new List<Message>()
            //    };
            //    var conversationIds = await GetPaginatedConversationIdsByUserIdAsync(userId);

            //    foreach (var conversationId in conversationIds)
            //    {
            //        var chatContextEntity = await GetEntityAsync<GptResponseEntity>(_configuration["StorageAcc:GPTContextTable"], userId, conversationId);
            //        if (chatContextEntity != null)
            //        {
            //            var previousMessages = JsonConvert.DeserializeObject<List<Message>>(chatContextEntity.UserContext);
            //            chatContext.Messages.AddRange(previousMessages);
            //        }
            //    }
            //}
            //else
            //{

            //    chatContext = new ChatContext()
            //    {
            //        Model = _configuration["OpenAI:Model"],
            //        Messages = new List<Message>()
            //    };
            //}

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
