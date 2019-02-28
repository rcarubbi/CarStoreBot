using Microsoft.Bot.Builder.FormFlow;
using System;

namespace CarStoreBot.Forms
{
    [Serializable]
    public class TicketForm
    {
        [Prompt("Could you please tell me your ticket number?")]
        public string TicketNumber { get; set; }

        internal static IForm<TicketForm> Build()
        {
            return new FormBuilder<TicketForm>()
                .Field(nameof(TicketNumber))
                .Build();
        }
    }
}