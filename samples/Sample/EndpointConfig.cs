using System;
using System.Configuration;
using Mandrill;
using Mandrill.Model;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Mandrill;

namespace Sample
{
    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server
    {
        public void Customize(BusConfiguration configuration)
        {
            var apiKey = ConfigurationManager.AppSettings["MANDRILL_API_KEY"];
            configuration.UseMandrill(apiKey: apiKey, replyResult: true);
            configuration.UsePersistence<InMemoryPersistence>();
        }
    }


    public class EmailSender : IWantToRunWhenBusStartsAndStops
    {
        public IBus Bus { get; set; }

        public void Start()
        {
            Console.WriteLine("Hit any key to send a email using the mandrill satellite");

            while (Console.ReadKey().Key.ToString().ToLower() != "q")
            {
                var mail = new MandrillMessage();
                mail.FromEmail = "mandrill.net@example.com";
                mail.AddTo("nservicebus@example.com", "Udi Dahan");
                mail.Subject = "NServiceBus.Mandrill test";
                mail.Text = "Hello NSericeBus! \nRegards";

                Bus.SendEmail(mail);
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
        public IBus Bus { get; set; }
        public IMandrillMessagesApi MandrillApi { get; set; }

        public EmailResultHandler(IBus bus, IMandrillMessagesApi mandrillApi)
        {
            Bus = bus;
            MandrillApi = mandrillApi;
        }

        public void Handle(MandrillEmailResult message)
        {
            //do something with the message result
            Logger.InfoFormat("{0} {1} {2}", message.Response.Id, message.Response.Status, message.Response.Email);

            //Wait 5 minutes, then try to get the email content back
            Bus.Defer(TimeSpan.FromMinutes(5), new GetMailContent() {MessageId = message.Response.Id});
        }

        public void Handle(GetMailContent message)
        {
            var content = MandrillApi.Content(message.MessageId);
            Logger.InfoFormat("Message id {0} sent at {1} had text content {2}", message.MessageId, content.Ts,
                content.Text);
        }

        private static readonly ILog Logger = LogManager.GetLogger(typeof (EmailResultHandler));
    }
}