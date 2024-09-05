// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.22.0

using EchoBot1.Servicos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot1.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {
        private readonly IStorageHelper _storageHelper;
        public DialogAndWelcomeBot(IStorageHelper storageHelper, ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger, storageHelper)
        {
            _storageHelper = storageHelper;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
           
                    // Check if the user already exists in storage
                    bool userExists = await _storageHelper.UserExistsAsync(turnContext.Activity.Recipient.Id);

                    if (userExists)
                    {
                        // User already exists, send a "welcome back" message
                        var welcomeBackMessage = $"Bem-vindo de volta! Em que posso ajudar hoje?";
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
                     
                        var reply = MessageFactory.Attachment(heroCard.ToAttachment(), "Iremos recolher os seus dados de Seguida.");
                   
                        await turnContext.SendActivityAsync(reply, cancellationToken);

                    }
                }
            }
        }
    

