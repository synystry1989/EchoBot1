namespace bot.Dialogs
{
    using EchoBot1;
    using EchoBot1.Dialogs;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using System.Threading;
    using System.Threading.Tasks;

    
        public class MainMenuDialog : ComponentDialog
        {

            public MainMenuDialog() : base(nameof(MainMenuDialog))
            {
                var waterfallSteps = new WaterfallStep[]
                {
                ShowMainMenuAsync,
                HandleMenuSelectionAsync,
                 
                };

                AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
                AddDialog(new TextPrompt(nameof(TextPrompt)));
            }

            private async Task<DialogTurnResult> ShowMainMenuAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
            var options = "Escolha uma das opções abaixo:\n" +
                      "1. Criar uma encomenda\n" +
                      "2. Consultar faturas\n" +
                      "3. Falar com um assistente\n" +
                      "4. Erros de sitema\n" +
                      "5. Consultar IA\n";
               
                     
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Bom dia, em que posso ajudar?"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(options), cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Digite o número da opção desejada:") }, cancellationToken);
            }

            public async Task<DialogContext> CreateContextAsync(ITurnContext turnContext, IStatePropertyAccessor<DialogState> conversationStateAccessor, CancellationToken cancellationToken)
            {
                var dialogSet = new DialogSet(conversationStateAccessor);
                dialogSet.Add(this);
                return await dialogSet.CreateContextAsync(turnContext, cancellationToken);
            }


            private async Task<DialogTurnResult> HandleMenuSelectionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
                var choice = stepContext.Result.ToString();
                switch (choice)
                {
                    case "1":
                        // Inicia o diálogo de criar encomenda
                        return await stepContext.BeginDialogAsync(nameof(OrderDialog), null, cancellationToken);
                    case "2":
                        // Inicia o diálogo de consultar faturas
                        return await stepContext.BeginDialogAsync(nameof(InvoiceDialog), null, cancellationToken);
                    case "3":
                        // Inicia o diálogo de suporte
                        return await stepContext.BeginDialogAsync(nameof(SupportDialog), null, cancellationToken);
                case "4":
                    // Inicia o diálogo de suporte
                    return await stepContext.BeginDialogAsync(nameof(IssueResolutionDialog), null, cancellationToken);
                case "5":
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Deixo-o á conversa com o nosso assistente artificial.muito obrigado!"), cancellationToken);
                        return await stepContext.EndDialogAsync();
                    default:
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Opção inválida, por favor, escolha uma opção válida."), cancellationToken);
                        return await stepContext.ReplaceDialogAsync(nameof(MainMenuDialog), null, cancellationToken);
                }
            }
        }
    }


