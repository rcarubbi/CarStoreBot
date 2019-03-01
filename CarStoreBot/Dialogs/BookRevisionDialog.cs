using CarStoreBot.Extensions;
using CarStoreBot.Factories;
using CarStoreBot.Forms;
using CarStoreBot.Integration.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarStoreBot.Dialogs
{
    [Serializable]
    public class BookRevisionDialog : IDialog<object>
    {
        private readonly string _preSelectedOption;
        private readonly DateTime _desiredDate;
        private readonly DateTime _desiredTime;

        private const string NEW_APPOINTMENT = "New";
        private const string MY_APPOINTMENTS = "My Appointments";
        
        private const string ANOTHER_TIME_OPTION = "Another time";

        public BookRevisionDialog(IDialogContext context, Dictionary<string, string> entities)
        {
            if (entities.ContainsKey(NLPDialog.DATE_ENTITY_NAME))
            {
                _desiredDate = DateTime.Parse(entities[NLPDialog.DATE_ENTITY_NAME]);
                _desiredTime = DateTime.Parse(entities[NLPDialog.DATE_ENTITY_NAME]);
                _preSelectedOption = NEW_APPOINTMENT;
            }
            
            if (entities.Count == 0)
            {
                _preSelectedOption = string.Empty;
            }
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (string.IsNullOrEmpty(_preSelectedOption))
            {
                await ShowOptions(context);
            }
            else if (_preSelectedOption == NEW_APPOINTMENT)
            {
                await context.PostAsync("Right, so first let's select a store.");
                context.Call(new SearchStoreDialog(true), AfterStoreSelected);
            }
        }

        private Task ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, OnSchedulingOptionSelected, new List<string> { NEW_APPOINTMENT, MY_APPOINTMENTS }, "Select option:", "It's not a valid option!");
            return Task.CompletedTask;
        }

        private async Task OnSchedulingOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var optionSelected = await result;

                switch (optionSelected)
                {
                    case NEW_APPOINTMENT:
                        await context.PostAsync("Right, so first let's select a store.");
                        context.Call(new SearchStoreDialog(true), AfterStoreSelected);
                        break;
                    case MY_APPOINTMENTS:
                        var service = AppointmentServiceFactory.Create();
                        var user = context.UserData.GetValue<User>("User");
                        var appointments = service.SearchAppointments(user.Email);
                        await ShowAppointments(context, appointments);
                        context.Done<object>(null);
                        break;
                    default:
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                System.Diagnostics.Trace.TraceError($"Error: {ex.Message}");
                await context.PostAsync("Oops! Too many attempts. Don't worry I'm checking what happened and you can try again in some minutes.");
            }
        }

        private async Task ShowAppointments(IDialogContext context, List<Appointment> appointments)
        {
            if (appointments.Count == 0)
            {
                await context.PostAsync("You don't have any pending appointment");
            }
            else
            {
                var user = context.UserData.GetValue<User>("User");
                var message = context.MakeMessage();
                message.AddAppointmentCard(user.Name, appointments);
                await context.PostAsync(message);
            }
        }

        private async Task AfterStoreSelected(IDialogContext context, IAwaitable<string> result)
        {
            var storeId = await result;
            if (!string.IsNullOrEmpty(storeId))
            {
                var service = SearchStoreServiceFactory.Create();
                var store = service.GetById(Convert.ToInt32(storeId));
                var appointment = new BookingForm() { Store = store, DesiredDate = _desiredDate, DesiredTime = _desiredTime };
                var form = new FormDialog<BookingForm>(appointment, BookingForm.BuildForm, FormOptions.PromptInStart);
                context.Call(form, AfterAppointmentScheduleFormFilled);
            }
            else
            {
                context.Done<object>(null);
            }
        }

        private async Task AfterAppointmentScheduleFormFilled(IDialogContext context, IAwaitable<BookingForm> result)
        {
            var form = await result;
            await Book(context, form);
        }

        private async Task Book(IDialogContext context, BookingForm form)
        {
            var desiredDatetime = Convert.ToDateTime($"{form.DesiredDate.ToShortDateString()} {form.DesiredTime.ToShortTimeString()}");
            var service = AppointmentServiceFactory.Create();
            var appointmentResult = service.TryBook(form.Store.Id, desiredDatetime);
            if (appointmentResult.Success)
            {
                await ShowAppointmentConfirmation(context, form);
                context.Done<object>(null);
            }
            else
            {
                context.PrivateConversationData.SetValue("BookingForm", form);
                ShowSuggestedTimes(context, appointmentResult.SuggestedTimes);
            }

        }

        private async Task ShowAppointmentConfirmation(IDialogContext context, BookingForm form)
        {
            await context.PostAsync($"Done! Your revision has been scheduled to {form.DesiredDate.ToShortDateString()} at {form.DesiredTime.ToShortTimeString()} on the store {form.Store.Name}.");
        }

        private void ShowSuggestedTimes(IDialogContext context, List<DateTime> suggestedTimes)
        {
            var options = new List<string>();
            options.AddRange(suggestedTimes.Select(h => h.ToString("dd/MM HH:mm")));
            options.Add(ANOTHER_TIME_OPTION);
            PromptDialog.Choice(context, AfterSuggestedDateTimeSelected, options, "Unfortunately this time is unavailable, however I have the some other times that maybe work for you...");
        }

        private async Task AfterSuggestedDateTimeSelected(IDialogContext context, IAwaitable<string> result)
        {
            var option = await result;
            var form = context.PrivateConversationData.GetValue<BookingForm>("BookingForm");
            if (option == ANOTHER_TIME_OPTION)
            {
                form.DesiredDate = default(DateTime);
                form.DesiredTime = default(DateTime);
                var dialog = new FormDialog<BookingForm>(form, BookingForm.BuildForm, FormOptions.PromptInStart);
                context.Call(dialog, AfterAppointmentScheduleFormFilled);
            }
            else
            {

                form.DesiredDate = Convert.ToDateTime(option);
                form.DesiredTime = Convert.ToDateTime(option);
                await Book(context, form);
            }
        }
    }
}