using Autofac;
using CarStoreBot.BotOverrides.Autofac;
using CarStoreBot.State;
using Microsoft.Bot.Builder.Dialogs;
using System.Reflection;
using System.Web.Http;

namespace CarStoreBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Conversation.UpdateContainer(builder =>
            {
                builder.RegisterModule(new DefaultExceptionMessageOverrideModule());
                var resolveAssembly = Assembly.GetCallingAssembly();
                builder.RegisterModule(new SqlBotDataStoreModule(resolveAssembly));

            });

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
