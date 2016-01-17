using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Mandrill;

namespace Tests
{
    internal class TestMode : Feature
    {
        public TestMode()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var q = context.Settings.Get<Address>("MasterNode.Address").SubScope("Mandrill");
            MandrillBusExtensions.GetMandrillQueueFunc = bus => q;
        }
    }
}