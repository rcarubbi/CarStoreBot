using Microsoft.ProjectOxford.Text.Core;
using Microsoft.ProjectOxford.Text.Language;
using Microsoft.ProjectOxford.Text.Sentiment;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace CarStoreBot.Services
{
    public class TextAnalytics
    {
        private readonly string _subKey;

        public TextAnalytics()
        {
            _subKey = ConfigurationManager.AppSettings["TextSentimentServiceSubKey"];
        }

        public async Task<float> MakeRequest(string text)
        {
            var language = await GetLanguage(text);

            var document = new SentimentDocument()
            {
                Id = Guid.NewGuid().ToString(),
                Text = text,
                Language = language
            };

            var request = new SentimentRequest();

            request.Documents.Add(document);

            var client = new SentimentClient(_subKey)
            {
                Url = ConfigurationManager.AppSettings["TextSentimentServiceUrl"] + "/sentiment"
            };

            var response = await client.GetSentimentAsync(request);

            var score = response.Documents[0].Score * 100;

            return score;
        }

        private async Task<string> GetLanguage(string text)
        {
            var document = new Document() { Id = Guid.NewGuid().ToString(), Text = text };

            var request = new LanguageRequest();

            request.Documents.Add(document);

            var client = new LanguageClient(_subKey)
            {
                Url = ConfigurationManager.AppSettings["TextSentimentServiceUrl"] + "/languages"
            };

            var response = await client.GetLanguagesAsync(request);

            return response.Documents[0].DetectedLanguages[0].Iso639Name;
        }
    }
}
