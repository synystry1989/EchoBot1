namespace bot.Dialogs
{
    using bot.Dialogs;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using System.Threading;
    using System.Threading.Tasks;

    

    public class OrderDialog : ComponentDialog
    {
        public OrderDialog() : base(nameof(OrderDialog))
        {
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
                // Lógica para criar a encomenda
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Encomenda de {quantity} unidade(s) de {product} criada com sucesso!"), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Encomenda cancelada."), cancellationToken);
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }


    public class CancelOrderDialog : ComponentDialog
    {
        private readonly OrderService _orderService;

        public CancelOrderDialog(OrderService orderService) : base(nameof(CancelOrderDialog))
        {
            _orderService = orderService;

            var waterfallSteps = new WaterfallStep[]
            {
            Step1Async,
            FinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        private async Task<DialogTurnResult> Step1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é o ID da encomenda que você deseja cancelar?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var orderId = (string)stepContext.Result;

            // Adiciona a lógica para cancelar a encomenda no sistema
            var success = await _orderService.CancelOrderAsync(orderId);

            if (success)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Encomenda {orderId} cancelada com sucesso!"), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Falha ao cancelar a encomenda {orderId}. Tente novamente."), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmOrderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["quantity"] = (int)stepContext.Result;
            var product = (string)stepContext.Values["product"];
            var quantity = (int)stepContext.Values["quantity"];
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Você gostaria de encomendar {quantity} unidade(s) de {product}. Confirma? (sim/não)"), cancellationToken);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("") }, cancellationToken);
        }

        private async Task<DialogTurnResult> EndStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (((string)stepContext.Result).ToLower() == "sim")
            {
                var product = (string)stepContext.Values["product"];
                var quantity = (int)stepContext.Values["quantity"];
                // Lógica para criar a encomenda
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Encomenda de {quantity} unidade(s) de {product} criada com sucesso!"), cancellationToken);
            }
            else if (((string)stepContext.Result).ToLower() == "suporte")
            {
                // Encaminhar para agente real
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Encaminhando você para um agente real. Por favor, aguarde..."), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Encomenda cancelada."), cancellationToken);
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }



        public class ModifyOrderDialog : ComponentDialog
        {
            private readonly OrderService _orderService;

            public ModifyOrderDialog(OrderService orderService) : base(nameof(ModifyOrderDialog))
            {
                _orderService = orderService;

                var waterfallSteps = new WaterfallStep[]
                {
            Step1Async,
            Step2Async,
            FinalStepAsync
                };

                AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
                AddDialog(new TextPrompt(nameof(TextPrompt)));
            }

            private async Task<DialogTurnResult> Step1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é o ID da encomenda que você deseja modificar?") }, cancellationToken);
            }

            private async Task<DialogTurnResult> Step2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
                stepContext.Values["orderId"] = (string)stepContext.Result;
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é a nova quantidade?") }, cancellationToken);
            }

            private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
                var orderId = (string)stepContext.Values["orderId"];
                var newQuantity = int.Parse((string)stepContext.Result);

                // Adiciona a lógica para modificar a encomenda no sistema
                var success = await _orderService.ModifyOrderAsync(orderId, newQuantity);

                if (success)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Encomenda {orderId} modificada para quantidade {newQuantity} com sucesso!"), cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Falha ao modificar a encomenda {orderId}. Tente novamente."), cancellationToken);
                }

                return await stepContext.ReplaceDialogAsync(nameof(MainMenuDialog), null, cancellationToken);

            }
        }
    }
}





