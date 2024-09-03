using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;

public class OrderService
{
    private readonly HttpClient _httpClient;

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> CancelOrderAsync(string orderId)
    {
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
        // Lógica para criar uma nova encomenda e simular uma resposta
        await Task.Delay(100);
        return order; // Simulação de uma resposta bem sucedida
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
