using System;
using Mandrill;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Mandrill;

namespace NServiceBus
{
    public static class ConfigureMandrill
    {
        public static BusConfiguration UseMandrill(this BusConfiguration settings, string apiKey, bool replyResult = false)
        {
            if (apiKey == null) throw new ArgumentNullException("apiKey");
            return UseMandrill(settings, new MandrillApi(apiKey), replyResult);
        }

        public static BusConfiguration UseMandrill(this BusConfiguration settings, MandrillApi api, bool replyResult = false)
        {
            if (api == null) throw new ArgumentNullException("api");

            return UseMandrill(settings, api.Messages, replyResult);
        }

        private static BusConfiguration UseMandrill(this BusConfiguration settings, IMandrillMessagesApi mandrillApi, bool replyResult = false)
        {
            settings.GetSettings().Set("NServiceBus.Mandrill.ReplyResult", replyResult);
            
            settings.RegisterComponents(x => x.RegisterSingleton(mandrillApi));

            settings.EnableFeature<Features.Mandrill>();
            return settings;
        }
    }
}