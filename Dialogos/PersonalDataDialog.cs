using EchoBot1.Servicos;
using EchoBot1.Modelos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;

namespace EchoBot1.Dialogs
{
    public class PersonalDataDialog : ComponentDialog
    {
        private const string NameStepMsgText = "Como te chamas?";
        private const string EmailStepMsgText = "qual é o teu email?";
        private readonly IStorageHelper _storageHelper;
        private readonly IConfiguration _configuration;

        public PersonalDataDialog(IStorageHelper storageHelper, IConfiguration configuration)
            : base(nameof(PersonalDataDialog))
        {
            _storageHelper = storageHelper;
            _configuration = configuration;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NameStepAsync,
                EmailStepAsync,
                SaveDataStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = MessageFactory.Text(NameStepMsgText, NameStepMsgText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["nome"] = (string)stepContext.Result;

            var promptMessage = MessageFactory.Text(EmailStepMsgText, EmailStepMsgText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> SaveDataStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["nome"] = (string)stepContext.Result;

            var userName = (string)stepContext.Values["nome"];
            var userEmail = (string)stepContext.Values["email"];

            // Save user data using IStorageHelper
            await SaveUserDataAsync(stepContext.Context, userName, userEmail, cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks, {userName}. Your email {userEmail} has been saved."), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task SaveUserDataAsync(ITurnContext turnContext, string name, string email, CancellationToken cancellationToken)
        {
            var userId = turnContext.Activity.From.Id;
            var personalData = new PersonalDataEntity(userId, name, email);


            await _storageHelper.InsertEntityAsync(_configuration["StorageAcc:UserProfileTable"], new PersonalDataEntity()
            {
                PartitionKey = userId,
                RowKey = Guid.NewGuid().ToString(),
                Email = email,
                Name = name
            });
        }
    }
}

