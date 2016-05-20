using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mandrill.Model;
using NServiceBus.Mandrill;

namespace NServiceBus
{
    public static class MandrillBusExtensions
    {
        public static Task SendEmail(this IPipelineContext bus, MandrillMessage message)
        {
            var msg = new SendMandrillEmail(message);
            return Send(bus, msg);
        }

        public static Task SendEmailTemplate(this IPipelineContext bus, MandrillMessage message, string templateName,
            IList<MandrillTemplateContent> templateContents = null)
        {
            if (templateName == null)
            {
                throw new ArgumentNullException(nameof(templateName));
            }

            var msg = new SendMandrillEmail(message, templateName, templateContents);

            return Send(bus, msg);
        }

        private static Task Send(IPipelineContext bus, SendMandrillEmail msg)
        {
            return bus.SendLocal(msg);
        }

        public static Task SendEmail(this IMessageSession bus, MandrillMessage message)
        {
            var msg = new SendMandrillEmail(message);
            return Send(bus, msg);
        }

        public static Task SendEmailTemplate(this IMessageSession bus, MandrillMessage message, string templateName,
            IList<MandrillTemplateContent> templateContents = null)
        {
            if (templateName == null)
            {
                throw new ArgumentNullException(nameof(templateName));
            }

            var msg = new SendMandrillEmail(message, templateName, templateContents);

            return Send(bus, msg);
        }

        private static Task Send(IMessageSession bus, SendMandrillEmail msg)
        {
            return bus.SendLocal(msg);
        }
    }
}