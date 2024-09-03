using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;

namespace EchoBot1
{
    public class OrderService
    {
        private readonly HttpClient _httpClient;

        public OrderService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<bool> CancelOrderAsync(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                throw new ArgumentException("Order ID cannot be null or empty.", nameof(orderId));

            try
            {
                var requestUri = $"https://api.example.com/orders/cancel/{orderId}";
                var response = await _httpClient.DeleteAsync(requestUri);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao cancelar a encomenda: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ModifyOrderAsync(string orderId, int newQuantity)
        {
            if (string.IsNullOrEmpty(orderId))
                throw new ArgumentException("Order ID cannot be null or empty.", nameof(orderId));

            if (newQuantity <= 0)
                throw new ArgumentException("New quantity must be greater than zero.", nameof(newQuantity));

            try
            {
                var requestUri = $"https://api.example.com/orders/modify/{orderId}";
                var content = new StringContent(
                    JsonConvert.SerializeObject(new { Quantity = newQuantity }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync(requestUri, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao modificar a encomenda: {ex.Message}");
                return false;
            }
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            try
            {
                // Simula uma chamada de API para criar um pedido
                await Task.Delay(100); // Simulação de operação assíncrona

                // Retorne a ordem criada com sucesso
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar a encomenda: {ex.Message}");
                return null; // Retorna null em caso de falha
            }
        }

        public class Order
        {
            public int OrderId { get; set; }
            public string CustomerName { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public string Status { get; set; }
        }
    }
}
