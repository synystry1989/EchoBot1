using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using EchoBot1.Servicos;
using Microsoft.Bot.Builder;
using System;
using EchoBot1.Modelos;

namespace EchoBot1.Dialogs
{
    public class PersonalDataDialog : ComponentDialog
    {
        private readonly UserProfileService _userProfileService;

        public PersonalDataDialog(UserProfileService userProfileService) : base(nameof(PersonalDataDialog))
        {
            _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));

            var waterfallSteps = new WaterfallStep[]
            {
                CheckExistingUserDataAsync,
                AskForNameStepAsync,
                AskForEmailStepAsync,
                AskForAddressStepAsync,
                AskForPhoneStepAsync,
                AskForNIFStepAsync,
                SavePersonalDataStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CheckExistingUserDataAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userId = stepContext.Context.Activity.From.Id;
            var userProfile = await _userProfileService.GetUserProfileAsync(userId);

            if (userProfile != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Bem-vindo de volta, {userProfile.Nome}! Como posso ajudar você hoje?"), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é o seu nome?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["nome"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é o seu email?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForAddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["email"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é a sua morada?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForPhoneStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["morada"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é o seu número de telefone?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForNIFStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["telefone"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é o seu NIF?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> SavePersonalDataStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["nif"] = (string)stepContext.Result;

            var userId = stepContext.Context.Activity.From.Id;
            var conversationId = stepContext.Context.Activity.Conversation.Id;
            var userProfile = new UserProfileEntity(userId, userId) // Salvando apenas com o userId para o perfil do usuário
            {
                Nome = (string)stepContext.Values["nome"],
                Email = (string)stepContext.Values["email"],
                Morada = (string)stepContext.Values["morada"],
                Telefone = (string)stepContext.Values["telefone"],
                NIF = (string)stepContext.Values["nif"]
            };

            try
            {
                await _userProfileService.UpsertUserProfileAsync(userProfile);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Os seus dados pessoais foram salvos com sucesso."), cancellationToken);
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Erro ao salvar dados pessoais: {ex.Message}"), cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
