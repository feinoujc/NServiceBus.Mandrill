using Mandrill;
using NServiceBus.ConsistencyGuarantees;
using NServiceBus.Mandrill;
using NServiceBus.Routing;
using NServiceBus.Transports;

namespace NServiceBus.Features
{
    public class Mandrill : Feature
    {
        public Mandrill()
        {
            Defaults(settings =>
            {
                var conventions = settings.Get<Conventions>();
                conventions.AddSystemMessagesConventions(t => typeof (MandrillEmailResult).IsAssignableFrom(t));
                conventions.AddSystemMessagesConventions(t => typeof (SendMandrillEmail).IsAssignableFrom(t));
            });
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            string processorAddress;
            var pipeline = context.AddSatellitePipeline("Mandrill.net message processor",
                context.Settings.GetRequiredTransactionModeForReceives(),
                PushRuntimeSettings.Default, "Mandrill",
                out processorAddress);

            pipeline.Register("DispatchMandrillMessage",
                b => new MandrillMessageDispatcher(b.Build<IMandrillMessagesApi>(), context.Settings.Get<bool>("NServiceBus.Mandrill.ReplyResult")),
                "Dispatches messages");

            var routing = context.Settings.Get<UnicastRoutingTable>();
            routing.RouteToAddress(typeof (SendMandrillEmail), processorAddress);
            routing.RouteToAddress(typeof (MandrillEmailResult), context.Settings.LocalAddress());
        }
    }
}