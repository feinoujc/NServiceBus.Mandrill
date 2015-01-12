using System;
using Mandrill;
using NServiceBus.Mandrill;

namespace NServiceBus
{
    public static class ConfigureMandrill
    {
        public static BusConfiguration UseMandrill(this BusConfiguration settings, string apiKey, bool replyResult = false)
        {
            if (apiKey == null) throw new ArgumentNullException("apiKey");
            return UseMandrill(settings, new MandrillApi(apiKey));
        }

        public static BusConfiguration UseMandrill(this BusConfiguration settings, MandrillApi api, bool replyResult = false)
        {
            if (api == null) throw new ArgumentNullException("api");

            return UseMandrill(settings, api.Messages, replyResult);
        }

        private static BusConfiguration UseMandrill(this BusConfiguration settings, IMandrillMessagesApi mandrillApi, bool replyResult = false)
        {
            settings.RegisterComponents(x =>
            {
                x.RegisterSingleton(mandrillApi);
                x.ConfigureComponent<MandrillSatellite>(DependencyLifecycle.InstancePerCall);
                x.ConfigureProperty<MandrillSatellite>(satellite => satellite.ReplyResult, replyResult);
            });

            settings.EnableFeature<Features.Mandrill>();
            return settings;
        }
    }
}