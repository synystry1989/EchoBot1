using EchoBot1;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using System.Threading;

public class OrderDialog : ComponentDialog
{
    private readonly OrderService _orderService;

    public OrderDialog(OrderService orderService) : base(nameof(OrderDialog))
    {
        _orderService = orderService;

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
        stepContext.Values["product"] = (string)stepContext.Result;
        return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = MessageFactory.Text("Quantas unidades você gostaria?") }, cancellationToken);
    }

    private async Task<DialogTurnResult> ConfirmOrderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        stepContext.Values["quantity"] = (int)stepContext.Result;
        var product = (string)stepContext.Values["product"];
        var quantity = (int)stepContext.Values["quantity"];
        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Você gostaria de encomendar {quantity} unidade(s) de {product}. Confirma? (sim/não)"), cancellationToken);
        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("") }, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        if (((string)stepContext.Result).ToLower() == "sim")
        {
            var product = (string)stepContext.Values["product"];
            var quantity = (int)stepContext.Values["quantity"];

            // Chama o serviço para criar a ordem
            var orderCreated = await _orderService.CreateOrderAsync(new OrderService.Order
            {
                ProductName = product,
                Quantity = quantity,
                // outros campos...
            });

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Encomenda de {quantity} unidade(s) de {product} criada com sucesso!"), cancellationToken);
        }
        else
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Encomenda cancelada."), cancellationToken);
        }
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
