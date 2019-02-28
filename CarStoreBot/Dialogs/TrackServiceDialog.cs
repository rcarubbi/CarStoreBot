using CarStoreBot.Factories;
using CarStoreBot.Forms;
using CarStoreBot.Integration.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Threading.Tasks;

namespace CarStoreBot.Dialogs
{
    [Serializable]
    public class TrackServiceDialog : IDialog<bool>
    {
        public Task StartAsync(IDialogContext context)
        {
            PromptDialog.Confirm(context, HasProtocolNumber,
                "Do you have a ticket number?",
                "It's not a valid option!");

            return Task.CompletedTask;
        }
        private async Task HasProtocolNumber(IDialogContext context, IAwaitable<bool> result)
        {
            var answer = await result;

            if (answer)
            {
                var dialog = FormDialog.FromForm(TicketForm.Build, FormOptions.PromptInStart);
                context.Call(dialog, AfterFormFilled);
            }
            else
            {
                context.Done(false);
            }
        }
        private async Task AfterFormFilled(IDialogContext context, IAwaitable<TicketForm> result)
        {
            var form = await result;
            var service = TicketSearchServiceFactory.Create();
            var ticket = service.Search(form.TicketNumber);

            if (ticket != null)
            {
                await ShowTicket(context, ticket);
                context.Done(true);
            }
            else
            {
                await context.PostAsync("I couldn't find any ticket with this number...");
                context.Done(false);
            }
        }

        private static async Task ShowTicket(IDialogContext context, Ticket ticket)
        {
            await context.PostAsync($"We've found a service status for your car model {ticket.Vehicle.Model} chassis number {ticket.Vehicle.Chassis}, at {ticket.Store.Name}.");
            await context.PostAsync($"{ticket.Status} estimated to be finished in {ticket.FinishDate:dd/MM}.");
            await context.PostAsync($"Enter in contact with your consultant at {ticket.Store.Name} to check this date and get further information. ");
        }
    }
}