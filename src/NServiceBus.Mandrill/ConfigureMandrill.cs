using System;
using Mandrill;
using NServiceBus.Configuration.AdvanceExtensibility;

namespace NServiceBus
{
    public static class ConfigureMandrill
    {
        public static EndpointConfiguration UseMandrill(this EndpointConfiguration settings, string apiKey,
            bool replyResult = false)
        {
            if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
            return UseMandrill(settings, new MandrillApi(apiKey), replyResult);
        }

        public static EndpointConfiguration UseMandrill(this EndpointConfiguration settings, MandrillApi api,
            bool replyResult = false)
        {
            if (api == null) throw new ArgumentNullException(nameof(api));

            return UseMandrill(settings, api.Messages, replyResult);
        }

        private static EndpointConfiguration UseMandrill(this EndpointConfiguration settings,
            IMandrillMessagesApi mandrillApi, bool replyResult = false)
        {
            settings.GetSettings().Set("NServiceBus.Mandrill.ReplyResult", replyResult);

            settings.RegisterComponents(x => x.RegisterSingleton(mandrillApi));

            settings.EnableFeature<Features.Mandrill>();
            return settings;
        }
    }
}