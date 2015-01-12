using System;
using System.Collections.Generic;
using System.Linq;
using Mandrill.Model;
using NServiceBus.Unicast;

namespace NServiceBus.Mandrill
{
    public static class MandrillBusExtensions
    {
        public static Func<IBus, Address> GetMandrillQueueFunc = bus =>
        {
            var settings = ((UnicastBus) bus).Settings;

            bool enabled;
            if (settings.TryGet("NServiceBus.Features.Mandrill", out enabled) && enabled)
            {
                return settings.Get<Address>("MasterNode.Address").SubScope("Mandrill");

            }
            throw new InvalidOperationException("Mandrill not enabled. Enable using configuration.EnableFeature<Mandrill>()");
        };

        public static void SendEmail(this IBus bus, MandrillMessage message)
        {
            var msg = new SendMandrillEmail
            {
                Message = message
            };
            SendInternal(bus, msg);
        }


        public static void SendEmailTemplate(this IBus bus, MandrillMessage message, string templateName,
            IList<MandrillTemplateContent> templateContents = null)
        {
            if (templateName == null)
            {
                throw new ArgumentNullException("templateName");
            }

            var msg = new SendMandrillEmail
            {
                Message = message,
                TemplateName = templateName,
                TemplateContents = templateContents != null ? templateContents.ToList() : null
            };

            SendInternal(bus, msg);
        }


        private static void SendInternal(IBus bus, SendMandrillEmail msg)
        {
            bus.Send(GetMandrillQueueFunc(bus), msg);
        }
    }
}