﻿using Azure.Data.Tables;
using System.Threading.Tasks;

namespace EchoBot1.Servicos
{
    public interface IStorageHelper
    {
        Task InsertEntityAsync<T>(string tableName, T entity) where T : ITableEntity;
        Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();
        Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey);
    }
}
