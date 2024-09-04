using System;
using System.Collections.Generic;
using System.IO;

namespace EchoBot1.Modelos
{
    public class KnowledgeBase
    {
        private readonly Dictionary<string, List<string>> _responses;

        public KnowledgeBase()
        {
            _responses = new Dictionary<string, List<string>>();
        }

        // Load responses from text files into the knowledge base
        public void LoadResponses(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, "*.txt", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (var streamReader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var parts = line.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim().ToLower();
                            var response = parts[1].Trim();

                            if (!_responses.ContainsKey(key))
                            {
                                _responses[key] = new List<string>();
                            }
                            _responses[key].Add(response);
                        }
                    }
                }
            }
        }

        // Retrieve a response based on user input
        public string GetResponse(string userInput)
        {
            var key = userInput.ToLower();
            if (_responses.ContainsKey(key))
            {
                return string.Join("\n", _responses[key]);
            }
            return null;
        }

        public List<string> SearchKeys(string userInput)
        {
            var matchingKeys = new List<string>();

            foreach (var key in _responses.Keys)
            {
                if (userInput.ToLower().Contains(key.ToLower()))
                {
                    matchingKeys.Add(key);
                }
            }

            return matchingKeys;
        }

        // Add or update the knowledge base with new responses
        public void AddOrUpdateResponse(string folderPath, string entity, string group, string key, string response)
        {
            var entityPath = Path.Combine(folderPath, entity);
            var groupFilePath = Path.Combine(entityPath, $"{group}.txt");

            // Ensure the directory exists
            Directory.CreateDirectory(entityPath);

            // Load existing responses to append or update
            var responses = new Dictionary<string, List<string>>();

            if (File.Exists(groupFilePath))
            {
                using (var reader = new StreamReader(groupFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
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

            // Add the new response without overwriting existing ones
            if (!responses.ContainsKey(key.ToLower()))
            {
                responses[key.ToLower()] = new List<string>();
            }
            responses[key.ToLower()].Add(response);

            // Write the updated responses back to the file
            using (var writer = new StreamWriter(groupFilePath, false))
            {
                foreach (var kvp in responses)
                {
                    foreach (var resp in kvp.Value)
                    {
                        writer.WriteLine($"{kvp.Key} = {resp}");
                    }
                }
            }

            // Update the in-memory dictionary with the new response
            if (!_responses.ContainsKey(key.ToLower()))
            {
                _responses[key.ToLower()] = new List<string>();
            }
            _responses[key.ToLower()].Add(response);
        }
    }
}
