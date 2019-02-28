using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;

namespace CarStoreBot.Forms
{
    [Serializable]
    public class AuthenticationForm
    {
        [Prompt("Please type your email to I look up in my database")]
        public string Email { get; set; }

        public static IForm<AuthenticationForm> Build()
        {
            return new FormBuilder<AuthenticationForm>()
                .Field(nameof(Email))
                .Build();
        }
    }
}