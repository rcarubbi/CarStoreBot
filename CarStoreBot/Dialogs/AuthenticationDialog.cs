using CarStoreBot.Factories;
using CarStoreBot.Forms;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace CarStoreBot.Dialogs
{
    [Serializable]
    public class AuthenticationDialog : IDialog<bool>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            message.Text = "Hi, I am the Car Store bot, before start I need to know who are you.";
            await context.PostAsync(message);
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var unused = await result as IMessageActivity;
            var dialog = FormDialog.FromForm(AuthenticationForm.Build, FormOptions.PromptInStart);
            context.Call(dialog, AfterFormFilled);
        }

        private async Task AfterFormFilled(IDialogContext context, IAwaitable<AuthenticationForm> result)
        {
            var form = await result;
            var repo = UserRepositoryFactory.CreateUserRepository();
            var user = repo.Search(form.Email);
            if (user == null)
            {
                PromptDialog.Confirm(context, RetriesAfterAuthenticationFail,
                    "Unfortunately I couldn't find you. Do you wanna try again?",
                    "It's not a valid option!");
            }
            else
            {
                context.UserData.SetValue("User", user);
                context.Done(true);
            }
        }

        private async Task RetriesAfterAuthenticationFail(IDialogContext context, IAwaitable<bool> result)
        {
            try
            {
                var optionSelected = await result;

                if (optionSelected)
                {
                    var message = context.MakeMessage();
                    message.Text = "Right, let's try again so.";
                    await context.PostAsync(message);
                    var dialog = FormDialog.FromForm(AuthenticationForm.Build, FormOptions.PromptInStart);
                    context.Call(dialog, AfterFormFilled);
                }
                else
                {
                    context.Done(false);
                }
            }
            catch (TooManyAttemptsException ex)
            {
                System.Diagnostics.Trace.TraceError($"Error: {ex.Message}");
                await context.PostAsync($"Oops! Too many attempts. Don't worry I'm checking what happened and you can try again in some minutes");
            }
        }
    }
}