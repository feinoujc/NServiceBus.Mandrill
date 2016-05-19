using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Mandrill;
using Mandrill.Model;
using NServiceBus.Logging;
using NServiceBus.Mandrill;
using NServiceBus.Pipeline;

namespace NServiceBus.Features
{
    public class MandrillMessageDispatcher : PipelineTerminator<ISatelliteProcessingContext>
    {
        private readonly IMandrillMessagesApi _messagesApi;
        private readonly bool _replyResult;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (MandrillMessageDispatcher));

        public MandrillMessageDispatcher(IMandrillMessagesApi messagesApi, bool replyResult)
        {
            _messagesApi = messagesApi;
            _replyResult = replyResult;
        }

        protected override async Task Terminate(ISatelliteProcessingContext context)
        {
            var command = DeserializeMessageBody(context);

            Debug.Assert(command != null, "command != null");

            var results = await SendEmails(command).ConfigureAwait(false);

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

                if (_replyResult)
                {
                   //want to send back the reply to the main queue
                   throw new NotImplementedException("Can't dispatch replies yet");
                }
            }
            
        }

        private Task<IList<MandrillSendMessageResponse>> SendEmails(SendMandrillEmail command)
        {
            if (command.TemplateName != null)
            {
                return _messagesApi.SendTemplateAsync(command.GetMessage(), command.TemplateName,
                    command.TemplateContents);
            }
            else
            {
                return _messagesApi.SendAsync(command.GetMessage());
            }
        }

        private SendMandrillEmail DeserializeMessageBody(ISatelliteProcessingContext context)
        {
            throw new NotImplementedException("Can't deserialize messages yet");
        }
    }
}