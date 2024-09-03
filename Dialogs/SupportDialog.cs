using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder;
using EchoBot1.Servicos;
using System;  // Certifique-se de importar o namespace para SupportService

namespace EchoBot1.Dialogs
{
    public class SupportDialog : ComponentDialog
    {
        private readonly SupportService _supportService;

        public SupportDialog(SupportService supportService) : base(nameof(SupportDialog))
        {
            _supportService = supportService ?? throw new ArgumentNullException(nameof(supportService));

            var waterfallSteps = new WaterfallStep[]
            {
                AskForSupportTopicStepAsync,
                OfferLiveSupportStepAsync,
                ConfirmSupportRequestStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskForSupportTopicStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é o assunto do suporte?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> OfferLiveSupportStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["supportTopic"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Você deseja suporte ao vivo?"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Sim", "Não" })
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmSupportRequestStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = ((FoundChoice)stepContext.Result).Value;
            var supportTopic = (string)stepContext.Values["supportTopic"];
            var userId = stepContext.Context.Activity.From.Id; // Obtém o ID do usuário

            if (choice.ToLower() == "sim")
            {
                await _supportService.NotifyLiveSupportAgentAsync(userId, supportTopic);  // Notifica o agente humano
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Um de nossos assistentes entrará em contato em breve para suporte ao vivo."), cancellationToken);
            }
            else
            {
                await _supportService.RegisterSupportRequestAsync(userId, supportTopic, false);  // Registra a solicitação de suporte
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Você solicitou suporte para: {supportTopic}. Um de nossos assistentes entrará em contato em breve."), cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
