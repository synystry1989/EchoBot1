using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft;
namespace EchoBot1.Servicos
{
    public class StorageHelper : IStorageHelper
    {
        private readonly IConfiguration _configuration;
        public StorageHelper(IConfiguration configuracao)
        {
            _configuration = configuracao;
        }

        // insert entity to azure table storage
        public async Task<TableClient> GetTableClient(string tableName)
        {
            TableServiceClient tableServiceClient = new TableServiceClient(_configuration.GetConnectionString("StorageAcc"));

            TableClient tableClient = tableServiceClient.GetTableClient(tableName: tableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }
        //insert entity to azure table storage
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
        //get entity from azure table storage
        public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            try
            {
                TableClient table = await GetTableClient(tableName);
                return await table.GetEntityAsync<T>(partitionKey, rowKey);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Error getting entity from table storage: {ex.Message}");
                return null;
            }
        }
        //delete entity from azure table storage
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
    }
}
