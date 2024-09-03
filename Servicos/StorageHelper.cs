using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
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

        private async Task<TableClient> GetTableClientAsync(string tableName)
        {
            var connectionString = _configuration.GetConnectionString("StorageAcc");
            var tableServiceClient = new TableServiceClient(connectionString);
            var tableClient = tableServiceClient.GetTableClient(tableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        public async Task InsertEntityAsync<T>(string tableName, T entity) where T : ITableEntity
        {
            try
            {
                TableClient table = await GetTableClientAsync(tableName);
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
                var table = await GetTableClientAsync(tableName);
                var response = await table.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Entidade não encontrada
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting entity from table storage: {ex.Message}", ex);
            }
        }

        public async Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey)
        {
            try
            {
                var table = await GetTableClientAsync(tableName);
                await table.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error deleting entity from table storage: {ex.Message}", ex);
            }
        }

        // Novo método para obter todas as conversas de um usuário
        public async Task<IEnumerable<T>> GetEntitiesByPartitionKeyAsync<T>(string tableName, string partitionKey) where T : class, ITableEntity, new()
        {
            try
            {
                var table = await GetTableClientAsync(tableName);
                var entities = new List<T>();

                await foreach (var entity in table.QueryAsync<T>(e => e.PartitionKey == partitionKey))
                {
                    entities.Add(entity);
                }

                return entities;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting entities from table storage: {ex.Message}", ex);
            }
        }
    }
}
