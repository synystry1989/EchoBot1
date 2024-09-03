using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot1.Servicos
{
    public class KnowledgeLearningBaseService
    {
        private readonly Dictionary<string, string> _responses = new Dictionary<string, string>();
        private readonly object _lock = new object();  // Lock object for thread safety

        private readonly string _knowledgeBasePath;

        public KnowledgeLearningBaseService(IConfiguration configuration)
        {
            _knowledgeBasePath = configuration["KnowledgeBase:Path"];

            if (string.IsNullOrEmpty(_knowledgeBasePath) || !Directory.Exists(_knowledgeBasePath))
            {
                throw new DirectoryNotFoundException("O diretório da base de conhecimento não foi encontrado.");
            }
        }

        public List<string> SearchKeys(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                throw new ArgumentException("User input cannot be null or whitespace.", nameof(userInput));

            var matchingKeys = new List<string>();

            lock (_lock)  // Ensure thread safety while accessing _responses
            {
                foreach (var key in _responses.Keys)
                {
                    if (userInput.Contains(key, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingKeys.Add(key);
                    }
                }
            }

            return matchingKeys;
        }

        public async Task LoadResponsesAsync(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("Folder path cannot be null or whitespace.", nameof(folderPath));

            var files = Directory.GetFiles(folderPath, "*.txt", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var streamReader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = await streamReader.ReadLineAsync()) != null)
                    {
                        var parts = line.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim().ToLower();
                            var response = parts[1].Trim();

                            lock (_lock)  // Ensure thread safety while modifying _responses
                            {
                                if (!_responses.ContainsKey(key))
                                {
                                    _responses[key] = response;
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool TryGetResponse(string userInput, out string response)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                throw new ArgumentException("User input cannot be null or whitespace.", nameof(userInput));

            var matchingKeys = SearchKeys(userInput);

            lock (_lock)  // Ensure thread safety while accessing _responses
            {
                if (matchingKeys.Count > 0)
                {
                    response = _responses[matchingKeys[0].ToLower()];
                    return true;
                }
            }

            response = null;
            return false;
        }

        public async Task AddOrUpdateResponseAsync(string folderPath, string entity, string group, string key, string response, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(entity) || string.IsNullOrWhiteSpace(group) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(response))
                throw new ArgumentException("Parameters cannot be null or whitespace.");

            var entityPath = Path.Combine(folderPath, entity);
            var groupFilePath = Path.Combine(entityPath, $"{group}.txt");

            try
            {
                Directory.CreateDirectory(entityPath);

                var responses = new Dictionary<string, List<string>>();

                if (File.Exists(groupFilePath))
                {
                    using (var reader = new StreamReader(groupFilePath))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            var parts = line.Split(new[] { '=' }, 2);
                            if (parts.Length == 2)
                            {
                                var existingKey = parts[0].Trim().ToLower();
                                var existingResponse = parts[1].Trim();

                                if (!responses.ContainsKey(existingKey))
                                {
                                    responses[existingKey] = new List<string>();
                                }
                                responses[existingKey].Add(existingResponse);
                            }
                        }
                    }
                }

                if (!responses.ContainsKey(key.ToLower()))
                {
                    responses[key.ToLower()] = new List<string>();
                }
                responses[key.ToLower()].Add(response);

                using (var writer = new StreamWriter(groupFilePath, false))
                {
                    foreach (var kvp in responses)
                    {
                        foreach (var resp in kvp.Value)
                        {
                            await writer.WriteLineAsync($"{kvp.Key} = {resp}");
                        }
                    }
                }

                lock (_lock)  // Ensure thread safety while modifying _responses
                {
                    _responses[key.ToLower()] = response;
                }
            }
            catch (IOException ioEx)
            {
                throw new Exception($"I/O error while updating knowledge base: {ioEx.Message}", ioEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error while updating knowledge base: {ex.Message}", ex);
            }
        }
    }
}
