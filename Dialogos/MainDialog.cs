using EchoBot1.Modelos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System.Threading.Tasks;
using System.Threading;
using System;
using EchoBot1.Servicos;


namespace EchoBot1.Dialogos
{
    public class MainDialog : ComponentDialog
    {
        private readonly IStorageHelper storageHelper;
        private readonly ModoAprendizagemDialog _modoAprendizagemDialog;
        private readonly UserProfileStoreDialog _userProfileStoreDialog;
        private readonly ILogger<MainDialog> _logger;
        private readonly KnowledgeBase _knowledgeBase;
        public MainDialog(KnowledgeBase knowledgeBase, UserProfileStoreDialog userProfileStore, ModoAprendizagemDialog modoAprendizagem, ILogger<MainDialog> logger)
           : base(nameof(MainDialog))
        {
            _knowledgeBase = knowledgeBase;
            _userProfileStoreDialog = userProfileStore;
            _logger = logger;
            _modoAprendizagemDialog = modoAprendizagem;


            AddDialog(new TextPrompt(nameof(TextPrompt)));
            //AddDialog(modoAprendizagem);
            //AddDialog(userProfileStore);

            var waterfallSteps = new WaterfallStep[]
            {
                IntroStepAsync,
            //    ActStepAsync,
               // FinalStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);



        }
        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        { //CHECK IF USER IS IN STORAGE ACCOUNT 
            var userId = stepContext.Context.Activity.From.Id;
            bool userProfile = await storageHelper.UserExistsAsync(userId);
            if (userProfile == null)
            {
                return await stepContext.BeginDialogAsync(nameof(UserProfileStoreDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(ModoAprendizagemDialog), null, cancellationToken);
            }


        }

      //  private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
       // {
            //    if (!_luisRecognizer.IsConfigured)
            //    {
            //        // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
            //        return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            //    }

            //    // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            //    var luisResult = await _luisRecognizer.RecognizeAsync<FlightBooking>(stepContext.Context, cancellationToken);
            //    switch (luisResult.TopIntent().intent)
            //    {
            //        case FlightBooking.Intent.BookFlight:
            //            await ShowWarningForUnsupportedCities(stepContext.Context, luisResult, cancellationToken);

            //            // Initialize BookingDetails with any entities we may have found in the response.
            //            var bookingDetails = new BookingDetails()
            //            {
            //                // Get destination and origin from the composite entities arrays.
            //                Destination = luisResult.ToEntities.Airport,
            //                Origin = luisResult.FromEntities.Airport,
            //                TravelDate = luisResult.TravelDate,
            //            };

            //            // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
            //            return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);

            //        case FlightBooking.Intent.GetWeather:
            //            // We haven't implemented the GetWeatherDialog so we just display a TODO message.
            //            var getWeatherMessageText = "TODO: get weather flow here";
            //            var getWeatherMessage = MessageFactory.Text(getWeatherMessageText, getWeatherMessageText, InputHints.IgnoringInput);
            //            await stepContext.Context.SendActivityAsync(getWeatherMessage, cancellationToken);
            //            break;

            //        default:
            //            // Catch all for unhandled intents
            //            var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
            //            var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
            //            await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
            //            break;
            //    }

            //    return await stepContext.NextAsync(null, cancellationToken);
            //}

            //private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            //{
            //    // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            //    // the Result here will be null.
            //    if (stepContext.Result is BookingDetails result)
            //    {
            //        // Now we have all the booking details call the booking service.

            //        // If the call to the booking service was successful tell the user.

            //        var timeProperty = new TimexProperty(result.TravelDate);
            //        var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
            //        var messageText = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
            //        var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            //        await stepContext.Context.SendActivityAsync(message, cancellationToken);
            //    }

            //    // Restart the main dialog with a different message the second time around
            //    var promptMessage = "What else can I do for you?";
            //    return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
            //}







        }
    }


