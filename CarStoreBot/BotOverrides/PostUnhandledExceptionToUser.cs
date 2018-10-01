using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace CarStoreBot.BotOverrides
{
    public class PostUnhandledExceptionToUser : IPostToBot
    {
        private readonly IPostToBot _inner;
        private readonly IBotToUser _botToUser;
        private readonly TraceListener _trace;

        public PostUnhandledExceptionToUser(IPostToBot inner, IBotToUser botToUser, ResourceManager resources, TraceListener trace)
        {
            SetField.NotNull(out this._inner, nameof(inner), inner);
            SetField.NotNull(out this._botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out _, nameof(resources), resources);
            SetField.NotNull(out this._trace, nameof(trace), trace);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            try
            {
                await _inner.PostAsync(activity, token);
            }
            catch (Exception error)
            {
                try
                {
                    if (Debugger.IsAttached)
                    {
                        var message = _botToUser.MakeMessage();
                        message.Text = $"Exception: { error.Message}";
                        message.Attachments = new[]
                        {
                            new Attachment(MediaTypeNames.Text.Plain, content: error.StackTrace)
                        };

                        await _botToUser.PostAsync(message, token);
                    }
                    else
                    {
                        await _botToUser.PostAsync("Ops! I'm still handle the previous message...", cancellationToken: token);
                    }
                }
                catch (Exception e)
                {
                    _trace.WriteLine(e);
                }

                throw;
            }
        }
    }
}