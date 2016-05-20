using System;
using System.Configuration;
using System.Threading.Tasks;
using Mandrill.Model;
using NServiceBus;

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
}