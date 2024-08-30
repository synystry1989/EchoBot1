using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using bot.Models;
using Microsoft.Bot.Builder;
using bot.Dialogs.Bot.Dialogs;

public class InvoiceDialog : ComponentDialog
{
    private readonly InvoiceActions _invoiceActions;

    public InvoiceDialog(InvoiceActions invoiceActions) : base(nameof(InvoiceDialog))
    {
        _invoiceActions = invoiceActions;

        var waterfallSteps = new WaterfallStep[]
        {
            AskCustomerNameAsync,
            ShowInvoicesAsync,
            ShowOrdersAsync
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));
    }

    private async Task<DialogTurnResult> AskCustomerNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        {
            Prompt = MessageFactory.Text("Por favor, forneça o nome do cliente para o qual você deseja ver as faturas.")
        }, cancellationToken);
    }

    private async Task<DialogTurnResult> ShowInvoicesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)

    { 
        var customerName = stepContext.Result.ToString();
        var invoices = _invoiceActions.GetInvoicesByCustomer(customerName);

        if (invoices != null && invoices.Any())
        {
            foreach (var invoice in invoices)
            {
                string invoiceDetails = $"Fatura para o Pedido ID {invoice.OrderId}:\nTotal: {invoice.Amount}\nData: {invoice.InvoiceDate.ToShortDateString()}\nProdutos:\n";

                foreach (var product in invoice.Products)
                {
                    invoiceDetails += $"- {product.Name}: {product.Cost}\n";
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(invoiceDetails), cancellationToken);
            }
        }
        else
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Nenhuma fatura encontrada para o cliente fornecido."), cancellationToken);
        }

        return await stepContext.NextAsync(cancellationToken: cancellationToken);
    }

    private async Task<DialogTurnResult> ShowOrdersAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var customerName = stepContext.Result.ToString();
        var orders = _invoiceActions.GetInvoicesByCustomer(customerName);

        if (orders != null && orders.Any())
        {
            foreach (var order in orders)
            {
                string orderDetails = $"Pedido ID {order.OrderId}:\nTotal: {order.Amount}\nData: {order.InvoiceDate.ToShortDateString()}\nProdutos:\n";

                foreach (var product in order.Products)
                {
                    orderDetails += $"- {product.Name}: {product.Cost}\n";
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(orderDetails), cancellationToken);
            }
        }
        else
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Nenhum pedido encontrado para o cliente fornecido."), cancellationToken);
        }

        return await stepContext.ReplaceDialogAsync(nameof(MainMenuDialog), null, cancellationToken);

    }
}
