using CarStoreBot.Extensions;
using CarStoreBot.Factories;
using CarStoreBot.Forms;
using CarStoreBot.Integration.Models;
using CarStoreBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CarStoreBot.Dialogs
{
    [Serializable]
    public class CheckGuaranteeDialog : IDialog<object>
    {
        private const string CHASSIS_OPTION = "Chassis";
        private const string PLATE_OPTION = "Plate";

        public async Task StartAsync(IDialogContext context)
        {
            await ShowOptions(context);
            context.Wait(MessageReceivedAsync);
        }

        private async Task ShowOptions(IDialogContext context)
        {
            var message = context.MakeMessage();
            message.Text = "How do you want me to search for your vehicle?";
            await context.PostAsync(message);
            await ShowCards(context);
        }

        private async Task ShowCards(IDialogContext context)
        {
            var message = context.MakeMessage();
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            var list = new List<Attachment>();

            var card = new ThumbnailCard
            {
                Title = "By Chassis",
                Subtitle = "Search vehicle by chassis",
                Images = new List<CardImage> { new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/chassis.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Chassis", null, CHASSIS_OPTION) }
            };
            list.Add(card.ToAttachment());

            card = new ThumbnailCard
            {
                Title = "By plate",
                Subtitle = "Search vehicle by plate",
                Images = new List<CardImage> { new CardImage($"{ConfigurationManager.AppSettings["BaseUrl"]}/img/plate.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Plate", null, PLATE_OPTION) }
            };
            list.Add(card.ToAttachment());


            message.Attachments = list;

            await context.PostAsync(message);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            if (await result is IMessageActivity activity)
                switch (activity.Text)
                {
                    case PLATE_OPTION:
                        var formPlate = FormDialog.FromForm(VehicleForm.BuildPlate, FormOptions.PromptInStart);
                        context.Call(formPlate, AfterFormFilled);
                        break;
                    case CHASSIS_OPTION:
                        var formChassis = FormDialog.FromForm(VehicleForm.BuildChassis, FormOptions.PromptInStart);
                        context.Call(formChassis, AfterFormFilled);
                        break;
                    default:
                        break;
                }
        }

        private async Task AfterFormFilled(IDialogContext context, IAwaitable<VehicleForm> result)
        {
            var form = await result;
            await SearchVehicle(context, form);
        }

        private async Task SearchVehicle(IDialogContext context, VehicleForm form)
        {
            var searchMode = string.Empty;
            var service = VehicleSearchServiceFactory.Create();

            Vehicle vehicle;
            if (!string.IsNullOrEmpty(form.Chassis))
            {
                searchMode = "the chassis";
                vehicle = service.SearchByChassis(form.Chassis);
            }
            else
            {
                searchMode = "the plate";
                vehicle = service.SearchByPlate(form.Plate);
            }

            var user = context.UserData.GetValue<User>("User");

            if (vehicle == null)
            {
                await context.PostAsync($"{"Mr.".GenderInflection(user)} {user.Name.FirstWord()}, {searchMode} that you informed is maybe wrong because I couldn't find your vehicle.");
                PromptDialog.Confirm(context, RetryFindVehicle, "Do you wanna try again?", "It's not a valid option!");

            }
            else if (vehicle.Warranties.Count == 0)
            {
                await context.PostAsync($"{"sr.".GenderInflection(user)} {user.Name.FirstWord()}, I've found your {vehicle.Model} but unfortunately it's not included in any guarantee program.");
                context.Done<object>(null);
            }
            else
            {
                await ShowWarranties(context, vehicle);
                context.Done<object>(null);
            }
        }

        private async Task ShowWarranties(IDialogContext context, Vehicle vehicle)
        {
            var message = context.MakeMessage();
            message.AddWarrantyCard(vehicle);
            await context.PostAsync(message);
        }

        private async Task RetryFindVehicle(IDialogContext context, IAwaitable<bool> result)
        {
            try
            {
                var optionSelected = await result;

                if (optionSelected)
                {
                    var message = context.MakeMessage();
                    message.Text = "Right, so let's try again";
                    await context.PostAsync(message);
                    await ShowOptions(context);
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    context.Done<object>(null);
                }
            }
            catch (TooManyAttemptsException ex)
            {
                System.Diagnostics.Trace.TraceError($"Error: {ex.Message}");
                await context.PostAsync("Oops! Too many attempts. Don't worry I'm checking what happened and you can try again in some minutes.s");
            }
        }
    }
}