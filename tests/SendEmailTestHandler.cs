using Mandrill.Model;
using NServiceBus;
using NServiceBus.Mandrill;

namespace Tests
{
    internal class SendEmailTestHandler : IHandleMessages<MandrillSendTest.SendEmail>
    {
        public IBus Bus { get; set; }

        public void Handle(MandrillSendTest.SendEmail message)
        {
            var email = new MandrillMessage
            {
                Subject = "This is a test",
                Text = "Hello World"
            };
            Bus.SendEmail(email);
        }

        public void Handle(MandrillSendTest.SendTemplateEmail message)
        {
            var email = new MandrillMessage
            {
                Subject = "This is a test",
                Text = "Hello World"
            };
            Bus.SendEmailTemplate(email, "test-template");
        }
    }
}