using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mandrill;
using Mandrill.Model;
using Mandrill.Serialization;

using Newtonsoft.Json.Linq;
using NServiceBus.Logging;
using NServiceBus.Satellites;
using NServiceBus.Serialization;

namespace NServiceBus.Mandrill
{
    /// <summary>
    ///     Satellite implementation to handle <see cref="SendMandrillEmail" /> messages.
    /// </summary>
    public class MandrillSatellite : ISatellite
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (MandrillSatellite));

        public IMessageSerializer MessageSerializer { get; set; }
        public IMandrillMessagesApi MandrillApi { get; set; }
        public IBus Bus { get; set; }

        public MandrillSatellite()
        {
            Disabled = true;
        }

        /// <summary>
        ///     This method is called when a message is available to be processed.
        /// </summary>
        /// <param name="message">
        ///     The <see cref="TransportMessage" /> received.
        /// </param>
        public bool Handle(TransportMessage message)
        {
            SendMandrillEmail sendEmail;

            using (var stream = new MemoryStream(message.Body))
            {
                sendEmail = (SendMandrillEmail) MessageSerializer.Deserialize(stream, new[] {typeof (SendMandrillEmail)}).First();
            }

            var mandrillMessage = sendEmail.Message;

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Sending mandrill message api request: " +
                             JObject.FromObject(mandrillMessage, MandrillSerializer.Instance));
            }

            IList<MandrillSendMessageResponse> results;
            if (sendEmail.TemplateName != null)
            {
                results = MandrillApi.SendTemplate(mandrillMessage, sendEmail.TemplateName, sendEmail.TemplateContents);
            }
            else
            {
                results = MandrillApi.Send(mandrillMessage);
            }


            foreach (var result in results)
            {
                if (result.Status != MandrillSendMessageResponseStatus.Sent)
                {
                    Logger.WarnFormat("Email result status {0} for {1},{2} Reject reason {3}", result.Status,
                        result.Id, result.Email, result.RejectReason);
                }
                else
                {
                    Logger.InfoFormat("Email result status {0} for {1}, {2}", result.Status, result.Id,
                        result.Email);
                }

                if (ReplyResult)
                {
                    Bus.Send(ReplyAddress, new MandrillEmailResult {Response = result});
                }
            }

            return true;
        }

        /// <summary>
        ///     Starts the <see cref="ISatellite" />.
        /// </summary>
        public void Start()
        {
            Logger.Info("Satellite started");
        }

        /// <summary>
        ///     Stops the <see cref="ISatellite" />.
        /// </summary>
        public void Stop()
        {
            //no-op
        }

        /// <summary>
        ///     The <see cref="Address" /> for this <see cref="ISatellite" /> to use when receiving messages.
        /// </summary>
        public Address InputAddress { get; set; }

        /// <summary>
        ///     The <see cref="Address" /> for this <see cref="ISatellite" /> to use when receiving messages.
        /// </summary>
        public Address ReplyAddress { get; set; }

        /// <summary>
        ///     Set to <code>true</code> to disable this <see cref="ISatellite" />.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Returns the result of the api call as a message on the master node addresss
        /// </summary>
        public bool ReplyResult { get; set; }
    }
}