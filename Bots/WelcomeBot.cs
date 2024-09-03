using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using EchoBot1.Servicos;
using Microsoft.Bot.Builder;
using System;

namespace EchoBot1.Dialogs
{
    public class WelcomeBot : ComponentDialog
    {
        private readonly UserProfileService _userProfileService;

        public WelcomeBot(UserProfileService userProfileService, PersonalDataDialog personalDataDialog) : base(nameof(WelcomeBot))
        {
            _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));

            var waterfallSteps = new WaterfallStep[]
            {
                AskForNameStepAsync,
                SaveNameStepAsync,
                CallPersonalDataDialogStepAsync,
                EndWelcomeDialogStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(personalDataDialog ?? throw new ArgumentNullException(nameof(personalDataDialog)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskForNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = "Olá! Eu sou o VimapontoAI. Como você se chama?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(promptMessage) }, cancellationToken);
        }

        private async Task<DialogTurnResult> SaveNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is string userName && !string.IsNullOrWhiteSpace(userName))
            {
                var userId = stepContext.Context.Activity.From.Id;

                await _userProfileService.SaveUserNameAsync(userId, userName);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Obrigado, {userName}. Vamos salvar seus dados pessoais a seguir."), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Não entendi seu nome. Poderia tentar novamente?"), cancellationToken);
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }

        private async Task<DialogTurnResult> CallPersonalDataDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Inicia o diálogo para coleta de dados pessoais
            return await stepContext.BeginDialogAsync(nameof(PersonalDataDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> EndWelcomeDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // O WelcomeBot termina e passa o controle de volta para o MainDialog
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
