using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using EchoBot1.Servicos;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using System;

namespace EchoBot1.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly UserProfileService _userProfileService;
        private readonly IStorageHelper _storageHelper;

        public MainDialog(UserProfileService userProfileService, IStorageHelper storageHelper) : base(nameof(MainDialog))
        {
            _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
            _storageHelper = storageHelper ?? throw new ArgumentNullException(nameof(storageHelper));

            var waterfallSteps = new WaterfallStep[]
            {
                CheckUserProfileAsync,
                // Outros passos
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CheckUserProfileAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userId = stepContext.Context.Activity.From.Id;

            // Verifica se o perfil do usuário já existe
            var userProfile = await _userProfileService.GetUserProfileAsync(userId);
            if (userProfile != null)
            {
                // Carrega as conversas anteriores do usuário
                var conversations = await _userProfileService.GetConversationsForUserAsync(userId);
                foreach (var conversation in conversations)
                {
                    // Aqui você pode adicionar lógica para usar o contexto das conversas anteriores
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Carregando conversa anterior: {conversation.Model}"), cancellationToken);
                }

                // Continue com o fluxo normal
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                // Se o usuário não existe, inicia o diálogo de coleta de dados
                return await stepContext.BeginDialogAsync(nameof(PersonalDataDialog), null, cancellationToken);
            }
        }
    }
}
