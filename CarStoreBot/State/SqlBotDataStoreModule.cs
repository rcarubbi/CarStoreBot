using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System.Configuration;
using System.Reflection;
using Module = Autofac.Module;

namespace CarStoreBot.State
{
    public class SqlBotDataStoreModule : Module
    {
        public static readonly object KeyDataStore = new object();


        public SqlBotDataStoreModule(Assembly assembly)
        {

            SetField.NotNull(out assembly, nameof(assembly), assembly);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConnectorStore>()
                .AsSelf()
                .InstancePerLifetimeScope();


            SqlBotDataContext.AssertDatabaseReady();

            var store = new SqlServerBotDataStore(ConfigurationManager.ConnectionStrings["BotDataContextConnectionString"]
                .ConnectionString);


            builder.Register(c => store)
                .Keyed<IBotDataStore<BotData>>(KeyDataStore)
                .AsSelf()
                .SingleInstance();

            builder.Register(c => new CachingBotDataStore(c.ResolveKeyed<IBotDataStore<BotData>>(KeyDataStore),
                    CachingBotDataStoreConsistencyPolicy.LastWriteWins))
                .As<IBotDataStore<BotData>>()
                .AsSelf()
                .InstancePerLifetimeScope();
        }
    }
}
