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
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace EchoBot1.Dialogs
{
    public class PersonalDataDialog : ComponentDialog
    {
        private readonly ChatContext _chatContext;
        private const string NameStepMsgText = "Como te chamas?";
        private const string EmailStepMsgText = "qual é o teu email?";
        private readonly IStorageHelper _storageHelper;
        private readonly IConfiguration _configuration;
        public IConfiguration Configuration => _configuration;


        public PersonalDataDialog(IStorageHelper storageHelper, IConfiguration configuration, ChatContext chatContext)
            : base(nameof(PersonalDataDialog))
        {
            _chatContext = chatContext;
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
         

            // Inicializa o chatContext e armazena em stepContext.Values
            var chatContext = new ChatContext();
            stepContext.Values["chatContext"] = chatContext;

            // Adiciona a mensagem ao chatContext
            chatContext.Messages.Add(new Message() { Role = "Assistente", Content = NameStepMsgText });

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["nome"] = (string)stepContext.Result;
            
            // Recupera o chatContext de stepContext.Values
            var chatContext = (ChatContext)stepContext.Values["chatContext"];

            // Adiciona a resposta do usuário ao chatContext
            chatContext.Messages.Add(new Message() { Role = "User", Content = (string)stepContext.Values["nome"] });

            var promptMessage = MessageFactory.Text(EmailStepMsgText, EmailStepMsgText, InputHints.ExpectingInput);

            // Adiciona a nova mensagem do assistente ao chatContext
            chatContext.Messages.Add(new Message() { Role = "Assistente", Content = EmailStepMsgText });

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> SaveDataStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["email"] = (string)stepContext.Result;

            // Recupera o chatContext de stepContext.Values
            var chatContext = (ChatContext)stepContext.Values["chatContext"];

            // Adiciona a resposta do usuário ao chatContext
            chatContext.Messages.Add(new Message() { Role = "User", Content = (string)stepContext.Values["email"] });

            var userName = (string)stepContext.Values["nome"];
            var userEmail = (string)stepContext.Values["email"];

            // Salva os dados do usuário usando IStorageHelper
            await _storageHelper.SaveUserDataAsync(userName, userEmail, stepContext.Context.Activity.From.Id, stepContext.Context.Activity.Conversation.Id);

            // Salva o chatContext atualizado
            await _storageHelper.SaveChatContextToStorageAsync(_configuration["StorageAcc:GPTContextTable"], stepContext.Context.Activity.From.Id, stepContext.Context.Activity.Conversation.Id, chatContext);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Obrigado, {userName}. Seu email {userEmail} foi salvo."), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }
}
