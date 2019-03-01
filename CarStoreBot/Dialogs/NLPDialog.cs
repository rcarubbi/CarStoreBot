using CarStoreBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
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

        public const string DATE_ENTITY_NAME = "builtin.datetimeV2.date";
        public const string DATETIME_ENTITY_NAME = "builtin.datetimeV2.datetime";
        public const string DATETIMERANGE_ENTITY_NAME = "builtin.datetimeV2.datetimerange";

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
            result.TryFindEntity(DATETIME_ENTITY_NAME, out var desiredDateTimeRecommendation);
            result.TryFindEntity(DATETIMERANGE_ENTITY_NAME, out var desiredDateTimeRangeRecommendation);


            var desiredDateTimeResolution = (((desiredDateRecommendation ?? desiredDateTimeRecommendation)?.Resolution["values"] as List<object>)?[0] as Dictionary<string, object>)?["value"] ??
                                            ((desiredDateTimeRangeRecommendation?.Resolution["values"] as List<object>)?[0] as Dictionary<string, object>)?["start"];

           

            context.Done($"{MenuOptions.BookRevision.ToString()}_{DATE_ENTITY_NAME}|{desiredDateTimeResolution}");
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