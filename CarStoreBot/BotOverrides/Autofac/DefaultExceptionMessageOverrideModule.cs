using Autofac;
using Autofac.Builder;
using Microsoft.Bot.Builder.Dialogs.Internals;
using System;
using System.Linq;

namespace CarStoreBot.BotOverrides.Autofac
{
    public class DefaultExceptionMessageOverrideModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PostUnhandledExceptionToUser>().Keyed<IPostToBot>(typeof(PostUnhandledExceptionToUser))
                .InstancePerLifetimeScope();

            RegisterAdapterChain<IPostToBot>(builder,
                    typeof(EventLoopDialogTask),
                    typeof(SetAmbientThreadCulture),
                    typeof(PersistentDialogTask),
                    typeof(ExceptionTranslationDialogTask),
                    typeof(SerializeByConversation),
                    typeof(PostUnhandledExceptionToUser),
                    typeof(LogPostToBot)
                )
                .InstancePerLifetimeScope();
        }

        public static IRegistrationBuilder<TLimit, SimpleActivatorData, SingleRegistrationStyle>
            RegisterAdapterChain<TLimit>(ContainerBuilder builder, params Type[] types)
        {
            return
                builder
                    .Register(c =>
                    {
                        var service = default(TLimit);
                        return types.Aggregate(service, (current, t) => c.ResolveKeyed<TLimit>(t, TypedParameter.From(current)));
                    })
                    .As<TLimit>();
        }
    }
}