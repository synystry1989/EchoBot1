using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EchoBot1.Servicos;
using EchoBot1.Modelos;
using Newtonsoft.Json;
using Azure.Data.Tables;
using EchoBot1.Dialogs;

namespace EchoBot1.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {        private readonly IStorageHelper _storageHelper;
        public DialogAndWelcomeBot( IStorageHelper storageHelper, IConfiguration configuration, ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base( storageHelper,configuration, conversationState, userState, dialog, logger)
        {

            _storageHelper = storageHelper;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Check if the user already exists in storage
                    bool userExists = await _storageHelper.UserExistsAsync(member.Id);

                    if (userExists)
                    {
                        // User already exists, send a "welcome back" message
                        var welcomeBackMessage = $"Bem-vindo de volta, {member.Name}! Em que posso ajudar hoje?";
                        await turnContext.SendActivityAsync(MessageFactory.Text(welcomeBackMessage), cancellationToken);
                    }
                    else
                    {
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
                }
            }
        }

       

      
    }
}
