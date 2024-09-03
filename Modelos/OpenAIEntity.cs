using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EchoBot1.Modelos
{
    public class OpenAiEntity : IOpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public OpenAiEntity(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GetResponseAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            var requestBody = new
            {
                model = _configuration["OpenAI:Model"],
                prompt,
                max_tokens = int.Parse(_configuration["OpenAI:MaxTokens"]),
                temperature = float.Parse(_configuration["OpenAI:Temperature"]),
                top_p = float.Parse(_configuration["OpenAI:TopP"]),
                frequency_penalty = float.Parse(_configuration["OpenAI:FrequencyPenalty"]),
                presence_penalty = float.Parse(_configuration["OpenAI:PresencePenalty"]),
            };

            return await SendRequestAsync(requestBody);
        }

        public async Task<string> SendRequestAsync(object requestBody)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["OpenAI:ApiKey"]}");

            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_configuration["OpenAI:APIEndpoint"], requestContent);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                var responseObject = JObject.Parse(responseString);

                return responseObject["choices"]?[0]?["text"]?.ToString() ?? "No response generated.";
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request error: {ex.Message}");
                return "A network error occurred.";
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Parsing response error: {ex.Message}");
                return "An error occurred while processing the response.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return "An unexpected error occurred.";
            }
        }
    }

    public interface IOpenAIService
    {
        Task<string> GetResponseAsync(string prompt);
        Task<string> SendRequestAsync(object requestBody);
    }
}
