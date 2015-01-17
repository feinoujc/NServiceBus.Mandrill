using NServiceBus.Mandrill;

namespace NServiceBus.Features
{
    public class Mandrill : Feature
    {
        public Mandrill()
        {
            Defaults(settings =>
            {
                var conventions = settings.Get<Conventions>();
                conventions.AddSystemMessagesConventions(t => typeof(MandrillEmailResult).IsAssignableFrom(t));
                conventions.AddSystemMessagesConventions(t => typeof(SendMandrillEmail).IsAssignableFrom(t));
            });
        }


        protected override void Setup(FeatureConfigurationContext context)
        {
            var masterNodeAddress = context.Settings.Get<Address>("MasterNode.Address");
            var replyResult = context.Settings.GetOrDefault<bool>("NServiceBus.Mandrill.ReplyResult");
            var satelliteAddress = masterNodeAddress.SubScope("Mandrill");

            context.Container.ConfigureComponent<MandrillSatellite>(DependencyLifecycle.InstancePerCall)
                .ConfigureProperty(x => x.Disabled, false)
                .ConfigureProperty(x => x.InputAddress, satelliteAddress)
                .ConfigureProperty(x => x.ReplyAddress, masterNodeAddress)
                .ConfigureProperty(x => x.ReplyResult, replyResult);
        }
    }
}