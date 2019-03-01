using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CarStoreBot.Dialogs
{
    [Serializable]
    public class CheckProblemDialog : IDialog<object>
    {
        private readonly Dictionary<string, Tuple<string, string>> _problemsDictionary = new Dictionary<string, Tuple<string, string>>();

        public CheckProblemDialog()
        {
            _problemsDictionary.Add("engine_oil_pressure", new Tuple<string, string>("Engine oil pressure", "Lights up when the ignition is activated and goes off within a few seconds " +
                                                                                                            "after the engine starts.Otherwise, consult the Dealer Network or Authorized Repair Shop. " +
                                                                                                            "It may flash intermittently when the engine is idling; must be cleared when the engine speed increases. " +
                                                                                                            "If the light comes on when the vehicle is in motion, park it immediately and switch off the engine as there may have been an interruption in the operation of the lubrication system, " +
                                                                                                            "which may cause engine damage and wheel locking.If the wheels lock, depress the clutch pedal, " +
                                                                                                            "set the gearshift lever to neutral, and turn off the ignition. " +
                                                                                                            "More power will be needed to brake and move the steering wheel. " +
                                                                                                            "Consult the Network of Dealers or Authorized Offices"));

            _problemsDictionary.Add("break_system", new Tuple<string, string>("Break system", "If the light does not go out when the engine is running and the parking brake has not been applied, " +
                                                                                              "drive the vehicle carefully to the nearest Authorized Dealer or Service Center.In this condition, " +
                                                                                              "the brake pedal must be pressed down harder and the required braking distance will be greater.Avoid unnecessary risk in these situations, " +
                                                                                              "if the efficiency of the brake system has decreased, park the vehicle and ask for help.When the ignition is switched on, " +
                                                                                              "the warning lamp on the brake system also lights up after three seconds of the parking brake being applied. " +
                                                                                              "The light stays on if the parking brake is not fully released. " +
                                                                                              "If it remains on after the parking brake has been lowered and another three seconds of delay, " +
                                                                                              "this means that the vehicle has a brake problem"));

            _problemsDictionary.Add("low_battery", new Tuple<string, string>("Low Battery", "The battery charge lamp lights up quickly when the ignition is turned on and the engine is still not working, " +
                                                                                            "such as a check to show that the lamp is working.It should turn off when the engine is started. " +
                                                                                            "If the lamp stays on or comes on while driving, there may be a problem with the battery charging system. " +
                                                                                            "Check with a dealer. If you need to drive a short distance with this lamp on, " +
                                                                                            "make sure you have turned off all accessories such as audio system and air conditioning"));

            _problemsDictionary.Add("engine_cooling", new Tuple<string, string>("Engine Cooling", "Always pay attention to this indicator because excessive heat is one of the most dangerous factors for the integrity of your engine. " +
                                                                                                  "This indicator will blink when the engine is running if the coolant temperature is too high. " +
                                                                                                  "If the coolant temperature of the engine is too high, stop the vehicle and turn off the engine. " +
                                                                                                  "Danger to the engine. Check the coolant level. " +
                                                                                                  "The lamp should light up when the ignition is switched on and switch off soon after the engine starts. " +
                                                                                                  "Otherwise, consult the Dealer Network or Authorized Repair Shop."));

            _problemsDictionary.Add("revision", new Tuple<string, string>("Revision", "The car review period is near.Carry out scheduling at the Authorized Dealer."));

            _problemsDictionary.Add("fault_indicator", new Tuple<string, string>("Fault indicator", "The fault indicator light comes on when the ignition is switched on and during start and goes out a few seconds after the engine starts. " +
                                                                                                    "If the control indicator light comes on with the engine running, " +
                                                                                                    "there is a fault in the engine emission control system. " +
                                                                                                    "At this point, the electronic system switches to an emergency program that allows the route to continue. " +
                                                                                                    "Look for a Network of Dealers or Authorized Offices as soon as possible. " +
                                                                                                    "Do not drive for a long time with the fault indicator light on, as it will damage the catalytic converter, " +
                                                                                                    "increase fuel consumption and may also indicate a pollutant emission exceeding that allowed by legislation. " +
                                                                                                    "This fault indicator light may illuminate by itself or in conjunction with the electronic system fault and electronic engine immobilizer indicator light."));

            _problemsDictionary.Add("abs_system", new Tuple<string, string>("Abs Breaking System", "This lamp will come on every time the ignition is turned on and then off. " +
                                                                                                   "During an emergency braking, when the ABS system is running, " +
                                                                                                   "the bulb ! will flash and then turn off. " +
                                                                                                   "If you do not notice any of the earlier symptoms mentioned in this section under normal driving conditions, " +
                                                                                                   "or if the light! when the engine is running - and is not in the above condition, the ABS system may be damaged. " +
                                                                                                   "However, the braking system of the vehicle will continue to function. " +
                                                                                                   "Contact the Dealer Network or Authorized Repair and Inspection Workshops."));

            _problemsDictionary.Add("door_open", new Tuple<string, string>("Door open", "The lamp comes on whenever one or more doors, or the cargo compartment, are open or ajar."));

            _problemsDictionary.Add("low_fuel", new Tuple<string, string>("Low Fuel", "This indicator light should come on when the ignition key is turned on and then off. " +
                                                                                      "If this does not occur, contact the Dealer Network or Authorized Repair Center for repair. " +
                                                                                      "Also illuminates when the fuel level is below the reserve level. Fill tank immediately."));

        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Okay! I need a picture of your vehicle's panel to check for possible problems...");

            context.Wait(MessageReceivedAsync);

        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as IMessageActivity;

            if (activity.Attachments != null && activity.Attachments.Count > 0)
            {
                var predictionClient = new CustomVisionPredictionClient
                {
                    ApiKey = ConfigurationManager.AppSettings["CustomVision.PredictionApiKey"],
                    Endpoint = ConfigurationManager.AppSettings["CustomVision.PredictionDomain"],
                };

                try
                {
                    var contentUrl = activity.Attachments[0].ContentUrl;
#if DEBUG
                    contentUrl = "http://1.bp.blogspot.com/-TRqv60nIjQY/Ulcuj1IZPiI/AAAAAAAAncM/axfyNzJQ62c/s1600/98_GM_FGD_3686_internas_10-10-13.jpg";
#endif

                    var imagePrediction = await predictionClient.PredictImageUrlWithHttpMessagesAsync(
                        new Guid(ConfigurationManager.AppSettings["CustomVision.ProjectId"]),
                        new ImageUrl(contentUrl));

                    if (imagePrediction.Body.Predictions.Any(x =>
                        _problemsDictionary.Select(y => y.Key).Any(key => key == x.TagName) && x.Probability > .75))
                    {

                        await context.PostAsync("Hmm let's see... I found the following problem(s) in your vehicle");

                        var message = context.MakeMessage();
                        var attachments = new List<Attachment>();

                        message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                        foreach (var prediction in imagePrediction.Body.Predictions)
                        {
                            if (!_problemsDictionary.TryGetValue(prediction.TagName, out var description) ||
                                !(prediction.Probability > .75)) continue;

                            var card = new ThumbnailCard
                            {
                                Title = description.Item1,
                                Text = description.Item2,
                                Images = new List<CardImage>
                                {
                                    new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/{prediction.TagName}.jpg")
                                },
                                Buttons = new List<CardAction>
                                    {new CardAction(ActionTypes.PostBack, "Details", null, prediction.TagName)}
                            };

                            attachments.Add(card.ToAttachment());
                        }

                        var backCard = new ThumbnailCard
                        {
                            Title = "Back",
                            Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Back", null, "#Back") }
                        };

                        attachments.Add(backCard.ToAttachment());

                        message.Attachments = attachments;

                        await context.PostAsync(message);

                        context.Wait(OnProblemSelectedAsync);
                    }
                    else
                    {
                        await context.PostAsync("I couldn't find any problem to the picture you've sent!");

                        context.Done<object>(null);
                    }
                }
                catch (Exception ex)
                {
                    await context.PostAsync("I couldn't understand your picture. It looks like an invalid file");
                    context.Done<object>(null);
                }
            }
            else
            {
                await context.PostAsync("I'm waiting a picture from your vehicle's panel to help you with your problem");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task OnProblemSelectedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            switch (activity.Text)
            {
                case "#Back":
                {
                    await context.PostAsync("To further information, check your vehicle's manual. I hope it was helpful!");
                    context.Done<object>(null);
                    break;
                }
                default:
                {
                    if (_problemsDictionary.TryGetValue(activity.Text, out var description))
                    {
                        await context.PostAsync($"{description.Item1}: {description.Item2}");
                    }

                    context.Wait(OnProblemSelectedAsync);
                    break;
                }
            }
        }
    }
}