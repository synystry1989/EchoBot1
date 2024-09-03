using EchoBot1;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using System.Threading;
using System;

public class OrderDialog : ComponentDialog
{
    private readonly OrderService _orderService;

    public OrderDialog(OrderService orderService) : base(nameof(OrderDialog))
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

        var waterfallSteps = new WaterfallStep[]
        {
            AskProductStepAsync,
            AskQuantityStepAsync,
            ConfirmOrderStepAsync,
            FinalStepAsync
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
    }

    private async Task<DialogTurnResult> AskProductStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual produto você gostaria de encomendar?") }, cancellationToken);
    }

    private async Task<DialogTurnResult> AskQuantityStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var product = (string)stepContext.Result;

        if (string.IsNullOrEmpty(product))
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Você não inseriu um produto válido. Tente novamente."), cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(OrderDialog), null, cancellationToken);
        }

        stepContext.Values["product"] = product;
        return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = MessageFactory.Text("Quantas unidades você gostaria?") }, cancellationToken);
    }

    private async Task<DialogTurnResult> ConfirmOrderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var quantity = (int)stepContext.Result;
        var product = (string)stepContext.Values["product"];

        if (quantity <= 0)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Quantidade inválida. Por favor, insira uma quantidade maior que zero."), cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(OrderDialog), null, cancellationToken);
        }

        stepContext.Values["quantity"] = quantity;

        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Você gostaria de encomendar {quantity} unidade(s) de {product}. Confirma? (sim/não)"), cancellationToken);
        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("") }, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var confirm = ((string)stepContext.Result).ToLower();

        if (confirm == "sim")
        {
            var product = (string)stepContext.Values["product"];
            var quantity = (int)stepContext.Values["quantity"];

            // Chama o serviço para criar a ordem
            var orderCreated = await _orderService.CreateOrderAsync(new OrderService.Order
            {
                ProductName = product,
                Quantity = quantity,
                // Adicione outros campos necessários, se houver
            });

            if (orderCreated != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Encomenda de {quantity} unidade(s) de {product} criada com sucesso!"), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Houve um problema ao criar sua encomenda. Tente novamente mais tarde."), cancellationToken);
            }
        }
        else
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Encomenda cancelada."), cancellationToken);
        }

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
