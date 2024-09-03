using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Bot.Builder;
using EchoBot1.Servicos; // Certifique-se de incluir o namespace correto para InvoiceService e EmailService
using System.Net.Http;

namespace EchoBot1.Dialogs
{
    public class EmpresarialDialog : ComponentDialog
    {
        private readonly EmailService _emailService;
        private readonly InvoiceActions _invoiceActions;

        public EmpresarialDialog(
            PersonalDataDialog personalDataDialog,
            KnowledgeLearningBaseDialog learningDialog,
            EmailService emailService,
            SupportDialog supportDialog,
            InvoiceActions invoiceAct,
            OrderService orderService)
            : base(nameof(EmpresarialDialog))
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _invoiceActions = invoiceAct ?? throw new ArgumentNullException(nameof(invoiceAct));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SelectOperationStepAsync,
                ExecuteSelectedOperationStepAsync,
                FinalStepAsync
            }));

            AddDialog(personalDataDialog ?? throw new ArgumentNullException(nameof(personalDataDialog)));
            AddDialog(learningDialog ?? throw new ArgumentNullException(nameof(learningDialog)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new EmailDialog(_emailService)); // Passa o serviço de email como argumento
            AddDialog(supportDialog ?? throw new ArgumentNullException(nameof(supportDialog)));
            AddDialog(new InvoiceDialog(_invoiceActions)); // Passa o serviço de invoice como argumento
            AddDialog(new OrderDialog(orderService ?? throw new ArgumentNullException(nameof(orderService)))); // Exemplo de instância do OrderDialog com serviço

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SelectOperationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = "Você está no modo empresarial. Escolha uma operação: 'alterar dados pessoais', 'modo aprendizagem', 'enviar email', 'consultar invoice', 'pedir suporte', 'fazer ordens', 'resolver problemas'.";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(promptMessage) }, cancellationToken);
        }

        private async Task<DialogTurnResult> ExecuteSelectedOperationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is string userInput)
            {
                userInput = userInput.ToLower();

                switch (userInput)
                {
                    case "alterar dados pessoais":
                        return await stepContext.BeginDialogAsync(nameof(PersonalDataDialog), null, cancellationToken);
                    case "modo aprendizagem":
                        return await stepContext.BeginDialogAsync(nameof(KnowledgeLearningBaseDialog), null, cancellationToken);
                    case "enviar email":
                        return await stepContext.BeginDialogAsync(nameof(EmailDialog), null, cancellationToken);
                    case "consultar invoice":
                        return await stepContext.BeginDialogAsync(nameof(InvoiceDialog), null, cancellationToken);
                    case "pedir suporte":
                        return await stepContext.BeginDialogAsync(nameof(SupportDialog), null, cancellationToken);
                    case "fazer ordens":
                        return await stepContext.BeginDialogAsync(nameof(OrderDialog), null, cancellationToken);
                    // Adicione mais casos conforme necessário
                    default:
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Opção inválida. Por favor, escolha uma opção válida."), cancellationToken);
                        return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
                }
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Não entendi. Por favor, escolha uma operação válida."), cancellationToken);
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Modo empresarial concluído."), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
