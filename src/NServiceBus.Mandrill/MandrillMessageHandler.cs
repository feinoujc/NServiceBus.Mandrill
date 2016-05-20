using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Mandrill;
using Mandrill.Model;
using NServiceBus.Logging;
using NServiceBus.Settings;

namespace NServiceBus.Mandrill
{
    internal class MandrillMessageHandler : IHandleMessages<SendMandrillEmail>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (MandrillMessageHandler));
        private readonly IMandrillMessagesApi _messagesApi;
        private readonly ReadOnlySettings _settings;

        public MandrillMessageHandler(IMandrillMessagesApi messagesApi, ReadOnlySettings settings)
        {
            _messagesApi = messagesApi;
            _settings = settings;
        }

        public async Task Handle(SendMandrillEmail command, IMessageHandlerContext context)
        {
            Debug.Assert(command?.MessageBody != null, "command?.MessageBody != null");

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

                if (_settings.GetOrDefault<bool>("NServiceBus.Mandrill.ReplyResult"))
                {
                    await context.Reply(new MandrillEmailResult {Response = result});
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
            return _messagesApi.SendAsync(command.GetMessage());
        }
    }
}