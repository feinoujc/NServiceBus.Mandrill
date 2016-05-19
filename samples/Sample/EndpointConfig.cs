using System;
using System.Configuration;
using System.Threading.Tasks;
using Mandrill;
using Mandrill.Model;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Mandrill;

namespace Sample
{
 

    public class EmailSender
    {
        public async Task Start(IEndpointInstance endpoint)
        {
            Console.WriteLine("Hit any key to send a email using the mandrill satellite");

            while (Console.ReadKey().Key.ToString().ToLower() != "q")
            {
                var mail = new MandrillMessage();
                mail.FromEmail = "mandrill.net@example.com";
                mail.AddTo("nservicebus@example.com", "Udi Dahan");
                mail.Subject = "NServiceBus.Mandrill test";
                mail.Text = "Hello NSericeBus! \nRegards";

                await endpoint.SendEmail(mail);
            }
        }

        public void Stop()
        {
            //no-op
        }
    }


    internal class GetMailContent : ICommand
    {
        public string MessageId { get; set; }
    }

    internal class EmailResultHandler : IHandleMessages<MandrillEmailResult>, IHandleMessages<GetMailContent>
    {
        public IMandrillMessagesApi MandrillApi { get; set; }

        public EmailResultHandler(IMandrillMessagesApi mandrillApi)
        {
            MandrillApi = mandrillApi;
        }

        public async Task Handle(MandrillEmailResult message, IMessageHandlerContext context)
        {
            //do something with the message result
            Logger.InfoFormat("{0} {1} {2}", message.Response.Id, message.Response.Status, message.Response.Email);

            await context.Send(new GetMailContent() {MessageId = message.Response.Id});
        }

        public async Task Handle(GetMailContent message, IMessageHandlerContext context)
        {
            var content = await MandrillApi.ContentAsync(message.MessageId);
            Logger.InfoFormat("Message id {0} sent at {1} had text content {2}", message.MessageId, content.Ts,
                content.Text);
        }

        private static readonly ILog Logger = LogManager.GetLogger(typeof (EmailResultHandler));
    }
}