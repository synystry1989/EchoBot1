using EchoBot1.Modelos;
using EchoBot1.Servicos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot1.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {
        private readonly IStorageHelper _storageHelper;
        public DialogAndWelcomeBot(IStorageHelper storageHelper, IConfiguration configuration, ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(storageHelper, configuration, conversationState, userState, dialog, logger)
        {

            _storageHelper = storageHelper;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            //criar um perfil a cada entrada de usuario
            PersonalDataEntity userProfile = new PersonalDataEntity();

            //lista de user existentes na storage
            var users = await _storageHelper.GetPaginatedUserIdsAsync();

            foreach (var member in membersAdded)
            {//se nao existe id nos membros do canal
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    userProfile.Id = member.Id;
                   
                    //guardar user profile
                    _storageHelper.InsertUserAsync(userProfile.Id,turnContext.Activity.Conversation.Id);
                    // User is new, send a welcome card
                    var heroCard = new HeroCard
                    {
                        Title = "Bem-vindo ao seu assistente VMPontoAI",
                        Subtitle = "Ajuda Personalizada:",
                        Text = "Envio Emails | Realizar encomendas | Ver faturas | Falar c/ Suporte",
                        Images = new List<CardImage> { new CardImage("C:\\Users\\synys\\Pictures\\Screenshots\\VMP.png") },
                        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Os nossos Produtos", value: "https://docs.microsoft.com/bot-framework") },
                    };

                    var reply = MessageFactory.Attachment(heroCard.ToAttachment());
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
                else
                {
                    //se existe id igual ao id do membro que esta a entrar
                    userProfile.Id = turnContext.Activity.Recipient.Id;
                          
                    foreach (var user in  users)
                    {
                        if (userProfile.Id == user)
                        {
                            var welcomeBackMessage = $"Bem-vindo de volta, {userProfile.Name}!";
                            await turnContext.SendActivityAsync(MessageFactory.Text(welcomeBackMessage), cancellationToken);
                        }
                    }



                }


            }
        }




    }
}
