using System;
using Microsoft.Bot.Builder.FormFlow;

namespace CarStoreBot.Forms
{
    [Serializable]
    public class SearchStoreForm
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        [Prompt("Please, inform your postcode so I can look for a store around neighborhood.")]
        public string Postcode { get; set; }

        [Prompt("now inform your city so I can look for a store around it.")]
        public string City { get; set; }

        [Prompt("Please, inform your county")]
        public string County { get; set; }

        internal static IForm<SearchStoreForm> BuildPostcodeForm()
        {
            var form = new FormBuilder<SearchStoreForm>()
                .Field(nameof(Postcode))
                .Build();
            return form;
        }

        internal static IForm<SearchStoreForm> BuildCityAndCountyForm()
        {
            var form = new FormBuilder<SearchStoreForm>()
                .Field(nameof(County))
                .Field(nameof(City))
                .Build();
            return form;
        }
    }
}