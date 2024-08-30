
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
public class OrderService
{
    private readonly HttpClient _httpClient;

    // Construtor para injeção de dependência
    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // Método para cancelar uma encomenda
    public async Task<bool> CancelOrderAsync(string orderId)
    {
        try
        {
            // Substitua com a URL da sua API de backend para cancelar uma encomenda
            var requestUri = $"https://api.example.com/orders/cancel/{orderId}";

            var response = await _httpClient.DeleteAsync(requestUri);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            // Lidar com exceções, como logar erros
            Console.WriteLine($"Erro ao cancelar a encomenda: {ex.Message}");
            return false;
        }
    }

    // Método para modificar uma encomenda
    public async Task<bool> ModifyOrderAsync(string orderId, int newQuantity)
    {
        try
        {
            // Substitua com a URL da sua API de backend para modificar uma encomenda
            var requestUri = $"https://api.example.com/orders/modify/{orderId}";

            // Cria o conteúdo da requisição com a nova quantidade
            var content = new StringContent(
                JsonConvert.SerializeObject(new { Quantity = newQuantity }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync(requestUri, content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            // Lidar com exceções, como logar erros
            Console.WriteLine($"Erro ao modificar a encomenda: {ex.Message}");
            return false;
        }
    }


    // Simulação de um banco de dados em memória
    private static readonly List<Order> _orders = new List<Order>();
    private static int _nextOrderId = 1;

    // Criar um novo pedido
    public async Task<Order> CreateOrderAsync(Order order)
    {
        // Define um novo ID para o pedido
        order.OrderId = _nextOrderId++;
        _orders.Add(order);

        // Simula uma operação assíncrona
        await Task.Delay(100); // Simula atraso de operação
        return order;
    }

    // Modificar um pedido existente
    public async Task<bool> UpdateOrderAsync(Order updatedOrder)
    {
        var existingOrder = _orders.FirstOrDefault(o => o.OrderId == updatedOrder.OrderId);
        if (existingOrder == null)
        {
            return false; // Pedido não encontrado
        }

        // Atualiza as propriedades do pedido
        existingOrder.CustomerName = updatedOrder.CustomerName;
        existingOrder.ProductName = updatedOrder.ProductName;
        existingOrder.Quantity = updatedOrder.Quantity;
        existingOrder.Price = updatedOrder.Price;
        existingOrder.Status = updatedOrder.Status;

        // Simula uma operação assíncrona
        await Task.Delay(100); // Simula atraso de operação
        return true;
    }

    // Apagar um pedido existente
    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        var order = _orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            return false; // Pedido não encontrado
        }

        _orders.Remove(order);

        // Simula uma operação assíncrona
        await Task.Delay(100); // Simula atraso de operação
        return true;
    }

    // Consultar um pedido pelo ID
    public async Task<Order> GetOrderByIdAsync(int orderId)
    {
        var order = _orders.FirstOrDefault(o => o.OrderId == orderId);

        // Simula uma operação assíncrona
        await Task.Delay(100); // Simula atraso de operação
        return order;
    }

    // Consultar todos os pedidos
    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        // Simula uma operação assíncrona
        await Task.Delay(100); // Simula atraso de operação
        return _orders;
    }
    private async Task<DialogTurnResult> ConfirmOrderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Captura a quantidade e o produto das etapas anteriores
        stepContext.Values["quantity"] = (int)stepContext.Result;
        var product = (string)stepContext.Values["product"];
        var quantity = (int)stepContext.Values["quantity"];

        // Envia uma mensagem de confirmação ao usuário
        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Você gostaria de encomendar {quantity} unidade(s) de {product}. Confirma? (sim/não)"), cancellationToken);

        // Pede a confirmação do usuário
        var promptOptions = new PromptOptions
        {
            Prompt = MessageFactory.Text("Digite 'sim' para confirmar ou 'não' para cancelar.")
        };
        return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
    }
    private async Task<DialogTurnResult> EndlStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Verifica a resposta do usuário
        var response = ((string)stepContext.Result).ToLower();

        // Recupera os dados da encomenda
        var product = (string)stepContext.Values["product"];
        var quantity = (int)stepContext.Values["quantity"];

        if (response == "sim")
        {
            // Lógica para criar a encomenda
            // Aqui você pode chamar um serviço que cria a encomenda no seu sistema
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Encomenda de {quantity} unidade(s) de {product} criada com sucesso!"), cancellationToken);
        }
        else if (response == "suporte")
        {
            // Encaminha para um agente real
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Encaminhando você para um agente real. Por favor, aguarde..."), cancellationToken);
        }
        else
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Encomenda cancelada."), cancellationToken);
        }

        // Finaliza o diálogo
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }



    public class Order
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } // Status como "Criado", "Em Processamento", "Concluído", etc.
    }
}


