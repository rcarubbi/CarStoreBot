using CarStoreBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Threading.Tasks;

namespace CarStoreBot.Dialogs
{
    [Serializable]
    
    public class NLPDialog : LuisDialog<string>
    {
        public NLPDialog(params ILuisService[] services)
        : base(services)
        {
            
        }

        public const string DATE_ENTITY_NAME = "DesiredDate";
        public const string TIME_ENTITY_NAME = "DesiredTime";

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Sorry, say again please.");
            context.Done("None");
        }

        [LuisIntent("SearchStore")]
        public Task SearchStore(IDialogContext context, LuisResult result)
        {
            context.Done(MenuOptions.SearchStore.ToString());
            return Task.CompletedTask;
        }

        [LuisIntent("BookRevision")]
        public Task BookRevision(IDialogContext context, LuisResult result)
        {
            result.TryFindEntity(DATE_ENTITY_NAME, out var desiredDateRecommendation);
            result.TryFindEntity(TIME_ENTITY_NAME, out var desiredTimeRecommendation);
            context.Done($"{MenuOptions.BookRevision.ToString()}_{DATE_ENTITY_NAME}|{desiredDateRecommendation?.Entity}_{TIME_ENTITY_NAME}|{desiredTimeRecommendation?.Entity}");
            return Task.CompletedTask;
        }

        [LuisIntent("CheckProblem")]
        public Task CheckProblem(IDialogContext context, LuisResult result)
        {
            context.Done(MenuOptions.CheckProblem.ToString());
            return Task.CompletedTask;
        }

        [LuisIntent("CheckGuarantee")]
        public Task CheckGuarantee(IDialogContext context, LuisResult result)
        {
            context.Done(MenuOptions.CheckGuarantee.ToString());
            return Task.CompletedTask;
        }

        [LuisIntent("TrackService")]
        public Task TrackService(IDialogContext context, LuisResult result)
        {
            context.Done(MenuOptions.TrackService.ToString());
            return Task.CompletedTask;
        }
    }
}