using CarStoreBot.Factories;
using CarStoreBot.Forms;
using CarStoreBot.Integration.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.SelectableLocation;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace CarStoreBot.Dialogs
{
    [Serializable]
    public class SearchStoreDialog : IDialog<string>
    {
        private const string SHARE_OPTION = "Share";
        private const string POSTCODE_OPTION = "Postcode";
        private const string COUNTY_AND_CITY_OPTION = "CountyAndCity";
        private readonly bool _selectable;

        public SearchStoreDialog(bool selectable)
        {
            _selectable = selectable;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await ShowLocationOptions(context);
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as IMessageActivity;

            switch (activity.Text)
            {
                case SHARE_OPTION:
                    await ShareLocation(context);
                    context.Wait(LocationReceiveAsync);
                    break;
                case POSTCODE_OPTION:
                    var postcodeFormDialog = FormDialog.FromForm(SearchStoreForm.BuildPostcodeForm, FormOptions.PromptInStart);
                    context.Call(postcodeFormDialog, AfterFormFilled);
                    break;
                case COUNTY_AND_CITY_OPTION:
                    var countyAndCityFormDialog = FormDialog.FromForm(SearchStoreForm.BuildCityAndCountyForm, FormOptions.PromptInStart);
                    context.Call(countyAndCityFormDialog, AfterFormFilled);
                    break;
                default:
                    await context.PostAsync("Select one of above options or type /restart");
                    break;
            }
        }

        private static async Task ShareLocation(IDialogContext context)
        {
            await context.PostAsync("Then share your location now so I can search for some stores near you.");
        }

        private async Task LocationReceiveAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (activity.Entities.Count > 0)
            {
                var latitude = activity.Entities[0].Properties.Root["geo"]["latitude"].ToString();
                var longitude = activity.Entities[0].Properties.Root["geo"]["longitude"].ToString();

                var form = new SearchStoreForm
                {
                    Latitude = latitude,
                    Longitude = longitude
                };

                await SearchLocations(context, form);
            }
            else
            {
                PromptDialog.Confirm(context, RetryShareLocation, "I couldn't get your location, do you wanna try again?", "It's not a valid option!");
            }
        }

        private async Task RetrySearch(IDialogContext context, IAwaitable<bool> result)
        {
            try
            {
                var answer = await result;
                if (answer)
                {
                    await ShowLocationOptions(context);
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    context.Done(string.Empty);
                }
            }
            catch (TooManyAttemptsException ex)
            {
                Trace.TraceError($"Error: {ex.Message}");
                await context.PostAsync($"Oops! Too many attempts. Don't worry I'm checking what happened and you can try again in some minutes");
            }
        }

        private async Task RetryShareLocation(IDialogContext context, IAwaitable<bool> result)
        {
            try
            {
                var answer = await result;
                if (answer)
                {
                    await ShareLocation(context);
                    context.Wait(LocationReceiveAsync);
                }
                else
                {
                    await ShowLocationOptions(context);
                    context.Wait(MessageReceivedAsync);
                }
            }
            catch (TooManyAttemptsException ex)
            {
                Trace.TraceError($"Error: {ex.Message}");
                await context.PostAsync($"Oops! Too many attempts. Don't worry I'm checking what happened and you can try again in some minutes");
            }
        }

        private async Task AfterFormFilled(IDialogContext context, IAwaitable<SearchStoreForm> result)
        {
            var form = await result;
            await SearchLocations(context, form);
        }

        private async Task SearchLocations(IDialogContext context, SearchStoreForm form)
        {
            var service = SearchStoreServiceFactory.Create();
            List<Store> stores;

            if (!string.IsNullOrWhiteSpace(form.Postcode))
            {
                stores = service.Search(form.Postcode);
            }
            else if (!string.IsNullOrEmpty(form.City) && !string.IsNullOrEmpty(form.County))
            {
                stores = service.Search(form.County, form.City);
            }
            else
            { 
                stores = service.Search(Convert.ToDouble(form.Latitude), Convert.ToDouble(form.Longitude));
            }

            if (!stores.Any())
            {
                await context.PostAsync("Unfortunately I couldn't find any store near you.");
                if (!_selectable)
                    context.Done(string.Empty);
                else
                {
                    PromptDialog.Confirm(context, RetrySearch, "Do you wanna try again?",
                        "It's not a valid option!");
                }
            }
            else
            {
                await ShowStores(context, stores);
                if (_selectable)
                    context.Wait(AfterSelectLocation);
                else
                    context.Done(string.Empty);
            }

        }

        private static async Task AfterSelectLocation(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;
            context.Done(activity.Text);
        }

        private async Task ShowStores(IBotToUser context, IReadOnlyCollection<Store> stores)
        {
            var message = context.MakeMessage();
            var apiKey = WebConfigurationManager.AppSettings["BingMapsApiKey"];
            var cardBuilder = new LocationCardBuilder(apiKey);
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            var cards = cardBuilder.CreateHeroCards(ParseStoreLocations(stores), stores.Select(x => x.Name).ToArray(),
                _selectable ? stores.Select(x => x.Id.ToString()).ToArray() : null);

            foreach (var card in cards)
            {
                message.Attachments.Add(card.ToAttachment());
            }

            await context.PostAsync(message);
        }

        private static IList<Location> ParseStoreLocations(IEnumerable<Store> stores)
        {
            return stores.Select(store => new Location
                {
                    Address = new Microsoft.Bot.Builder.SelectableLocation.Address
                    {
                        FormattedAddress = store.Address,
                        AddressLine = store.Address
                    },
                    GeocodePoints = new List<GeocodePoint>
                    {
                        new GeocodePoint
                        {
                            Coordinates = new List<double>
                            {
                                store.Latitude,
                                store.Longitude
                            }
                        }
                    },
                    Name = store.Name,
                    Point = new GeocodePoint
                    {
                        Coordinates = new List<double>
                        {
                            store.Latitude,
                            store.Longitude
                        }
                    }
                })
                .ToList();
        }


        private static async Task ShowLocationOptions(IBotContext context)
        {
            var message = context.MakeMessage();
            message.Text = "To find a store I need your location, how do you want to share it?";
            await context.PostAsync(message);
            await ShowCards(context);
        }

        private static async Task ShowCards(IBotContext context)
        {
            var message = context.MakeMessage();
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            ThumbnailCard card = null;
            var list = new List<Attachment>();

            card = new ThumbnailCard
            {
                Title = "By Postcode",
                Subtitle = "Stores near your postcode",
                Images = new List<CardImage> { new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/postcode.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Type your postcode", null, POSTCODE_OPTION) }
            };
            list.Add(card.ToAttachment());
            card = new ThumbnailCard
            {
                Title = "By City and County",
                Subtitle = "Stores in your city",
                Images = new List<CardImage> { new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/countyCity.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Type your city and state", null, COUNTY_AND_CITY_OPTION) }
            };
            list.Add(card.ToAttachment());

            switch (context.Activity.ChannelId)
            {
                case "facebook":
                    card = new ThumbnailCard
                    {
                        Title = "By GPS Location sharing",
                        Subtitle = "Share your location through facebook",
                        Images = new List<CardImage> { new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/share.png") },
                        Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Share", null, SHARE_OPTION) }
                    };
                    list.Add(card.ToAttachment());
                    break;
                case "telegram":
                    card = new ThumbnailCard
                    {
                        Title = "By GPS Location sharing",
                        Subtitle = "Share your location through telegram",
                        Images = new List<CardImage> { new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/share.png") },
                        Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Share", null, SHARE_OPTION) }
                    };
                    list.Add(card.ToAttachment());
                    break;
            }

            message.Attachments = list;

            await context.PostAsync(message);
        }
    }
}