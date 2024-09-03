using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EchoBot1.Modelos;
using Azure;

namespace EchoBot1.Servicos
{
    public class UserProfileService
    {
        private readonly TableClient _tableClient;
        private readonly StorageHelper _storageHelper;

        public UserProfileService(IConfiguration configuration, string tableName, StorageHelper storageHelper)
        {
            _storageHelper = storageHelper;
            var serviceClient = new TableServiceClient(configuration["ConnectionStrings:StorageAcc"]);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task UpsertUserProfileAsync(UserProfileEntity userProfile)
        {
            if (userProfile == null)
            {
                throw new ArgumentNullException(nameof(userProfile));
            }

            await _tableClient.UpsertEntityAsync(userProfile, TableUpdateMode.Replace);
        }

        public async Task<UserProfileEntity> GetUserProfileAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.");
            }

            try
            {
                var response = await _tableClient.GetEntityAsync<UserProfileEntity>(userId, userId);
                return response?.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Usuário não encontrado
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user profile: {ex.Message}");
                throw;
            }
        }

        public async Task UpdatePersonalDataAsync(UserProfileEntity userProfile)
        {
            if (userProfile == null)
            {
                throw new ArgumentNullException(nameof(userProfile));
            }

            await UpsertUserProfileAsync(userProfile);
        }

        // Método para recuperar todas as conversas de um usuário
        public async Task<IEnumerable<ChatContext>> GetConversationsForUserAsync(string userId)
        {
            return await _storageHelper.GetEntitiesByPartitionKeyAsync<ChatContext>("Conversations", userId);
        }
    }
}
