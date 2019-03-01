using CarStoreBot.Integration.Models;
using CarStoreBot.Models;
using CarStoreBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis;

namespace CarStoreBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private MenuOptions _lastSelectedOption;

        public Task StartAsync(IDialogContext context)
        {
            _lastSelectedOption = MenuOptions.Authentication;
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (_lastSelectedOption == MenuOptions.Authentication)
            {
                await StartConversation(context, activity);
            }
            else
            {
                if (!Enum.TryParse<MenuOptions>(activity?.Text, out var selectedOption))
                {
                    selectedOption = MenuOptions.Other;
                }
                _lastSelectedOption = selectedOption;
                await CallDialog(context, activity, selectedOption, new Dictionary<string, string>());
            }
        }

        private async Task StartConversation(IDialogContext context, object message)
        {
            _lastSelectedOption = MenuOptions.Other;
            if (!context.UserData.TryGetValue<User>("User", out var user))
            {
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuthentication, message, CancellationToken.None);
            }
            else
            {
                await SendGreetings(context, user);
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task CallDialog(IDialogContext context, object message, MenuOptions option, Dictionary<string, string> entities)
        {
            switch (option)
            {
                case MenuOptions.SearchStore:
                    context.Call(new SearchStoreDialog(false), ResumeAfterDialog);
                    break;
                case MenuOptions.BookRevision:
                    context.Call(new BookRevisionDialog(context, entities), ResumeAfterDialog);
                    break;
                case MenuOptions.CheckProblem:
                    context.Call(new CheckProblemDialog(), ResumeAfterDialog);
                    break;
                case MenuOptions.CheckGuarantee:
                    context.Call(new CheckGuaranteeDialog(), ResumeAfterDialog);
                    break;
                case MenuOptions.TrackService:
                    context.Call(new TrackServiceDialog(), ResumeAfterTrackService);
                    break;
                case MenuOptions.Other:
                    await context.Forward(new NLPDialog(
                        new LuisService(
                            new LuisModelAttribute(
                                ConfigurationManager.AppSettings["Luis.ModelId"],
                                ConfigurationManager.AppSettings["Luis.SubscriptionKey"],
                                LuisApiVersion.V2,
                                ConfigurationManager.AppSettings["Luis.Domain"]))), ResumeAfterNlp, message, CancellationToken.None);
                    break;
                case MenuOptions.Authentication:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }
        }

        private async Task ResumeAfterNlp(IDialogContext context, IAwaitable<string> result)
        {
            var nlpResult = await result;
            var parts = nlpResult.Split('_');
            var entities = ExtractEntities(parts.Skip(1).ToList());

            if (!Enum.TryParse<MenuOptions>(parts[0], out var option))
            {
                _lastSelectedOption = MenuOptions.Other;
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                _lastSelectedOption = option;
                await CallDialog(context, context.Activity.AsMessageActivity(), option, entities);
            }
        }

        private static Dictionary<string, string> ExtractEntities(IEnumerable<string> list)
        {
            return list.Select(item => item.Split('|'))
                .Where(parts => !string.IsNullOrWhiteSpace(parts[1]))
                .ToDictionary(parts => parts[0], parts => parts[1]);
        }


        private async Task ResumeAfterTrackService(IDialogContext context, IAwaitable<bool> result)
        {
            var ticketFound = await result;
            if (ticketFound)
            {
                await context.PostAsync("What do you think about my service?");
                context.Wait(SurveyReceivedAsync);
            }
            else
            {
                await TransferToHuman(context);
                _lastSelectedOption = MenuOptions.Authentication;
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterAuthentication(IDialogContext context, IAwaitable<bool> result)
        {
            var authenticated = await result;
            if (Convert.ToBoolean(authenticated))
            {
                await SendGreetings(context, context.UserData.GetValue<User>("User"));
            }
            else
            {
                await EndConversation(context);
                _lastSelectedOption = MenuOptions.Authentication;
            }
            context.Wait(MessageReceivedAsync);
        }

        private async Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync("What do you think about my service?");
            context.Wait(SurveyReceivedAsync);
        }

        private async Task SurveyReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            var text = activity?.Text;

            var textAnalisysService = new TextAnalytics();
            var score = await textAnalisysService.MakeRequest(text);

            if (score < 50)
            {
                await TransferToHuman(context);
                _lastSelectedOption = MenuOptions.Authentication;
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                PromptDialog.Confirm(context, ResumeAfterContinueQuestion,
                    "Thanks for your answer! Can I help you with anything else?",
                    "It's not a valid option!");
            }
        }

        private async Task ResumeAfterContinueQuestion(IDialogContext context, IAwaitable<bool> result)
        {
            var answer = await result;
            if (answer)
            {
                await context.PostAsync("Okay, so tell me what else I can do for you.");
                await ShowCards(context);
            }
            else
            {
                await EndConversation(context);
                _lastSelectedOption = MenuOptions.Authentication;
            }
            context.Wait(MessageReceivedAsync);
        }

        private static async Task EndConversation(IDialogContext context)
        {
            await context.PostAsync("Thanks for your contact. I'm always here to help you.");
        }

        private static async Task TransferToHuman(IDialogContext context)
        {
            await context.PostAsync("Ok, so I will transfer you to one of our agents. Wait please...");
        }
        public async Task SendGreetings(IDialogContext context, User user)
        {
            var message = context.MakeMessage();

            message.Text = $"Hi {user.Name.FirstWord()}, how can I help you today? (Select one option below or just type anything you want)";
            await context.PostAsync(message);
            await ShowCards(context);
        }

        private static async Task ShowCards(IBotToUser context)
        {
            var message = context.MakeMessage();

            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            var list = new List<Attachment>();

            var card = new ThumbnailCard
            {
                Title = "Search Store",
                Subtitle = "Find a store near you",
                Images = new List<CardImage> { new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/SearchStore.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Search", null, MenuOptions.SearchStore.ToString()) }
            };
            list.Add(card.ToAttachment());

            card = new ThumbnailCard
            {
                Title = "Book a revision",
                Subtitle = "Book your vehicle revision",
                Images = new List<CardImage> { new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/BookRevision.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Book", null, MenuOptions.BookRevision.ToString()) }
            };
            list.Add(card.ToAttachment());

            card = new ThumbnailCard
            {
                Title = "Check problems",
                Subtitle = "Send me a picture from your panel and I'll try to identify your problem",
                Images = new List<CardImage> { new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/CheckProblem.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Check", null, MenuOptions.CheckProblem.ToString()) }
            };
            list.Add(card.ToAttachment());
            card = new ThumbnailCard
            {
                Title = "Check Guarantee",
                Subtitle = "Check your vehicle guarantee",
                Images = new List<CardImage> { new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/CheckGuarantee.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Check", null, MenuOptions.CheckGuarantee.ToString()) }
            };
            list.Add(card.ToAttachment());

            card = new ThumbnailCard
            {
                Title = "Track Service",
                Subtitle = "Track your in progress vehicle repair",
                Images = new List<CardImage> { new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/TrackService.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Track", null, MenuOptions.TrackService.ToString()) }
            };
            list.Add(card.ToAttachment());

            message.Attachments = list;

            await context.PostAsync(message);
        }
    }
}