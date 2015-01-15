using System;
using System.IO;
using Mandrill.Model;
using Mandrill.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NServiceBus;
using NServiceBus.Mandrill;
using NServiceBus.Testing;
using NUnit.Framework;


namespace Tests
{
    [TestFixture]
    internal class MandrillSendTest
    {
        static MandrillSendTest()
        {
            Test.Initialize(config =>
            {
                config.AssembliesToScan(typeof(SendEmailTestHandler).Assembly,
                    typeof(NServiceBus.Features.Mandrill).Assembly);
                config.EnableFeature<NServiceBus.Features.Mandrill>();
            });

        }

        [Test]
        public void Can_send_email()
        {
            Test.Handler<SendEmailTestHandler>()
                .ExpectSendToDestination<SendMandrillEmail>((message, address) =>
                {

                    var body = message.GetMessage();

                    Assert.AreEqual("Hello World", body.Text);
                    Assert.AreEqual("This is a test", body.Subject);
                    Assert.IsTrue(address.Queue.EndsWith(".mandrill", StringComparison.OrdinalIgnoreCase));
                    
                    return true;
                })
                .OnMessage<SendEmail>(email => { });
        }

        [Test]
        public void Can_send_template_email()
        {
            Test.Handler<SendEmailTestHandler>()
                .ExpectSendToDestination<SendMandrillEmail>((message, address) =>
                {
                    var body = message.GetMessage();

                    Assert.AreEqual("Hello World", body.Text);
                    Assert.AreEqual("This is a test", body.Subject);
                    Assert.AreEqual("test-template", message.TemplateName);
                    Assert.IsTrue(address.Queue.EndsWith(".mandrill", StringComparison.OrdinalIgnoreCase));

                    return true;
                })
                .OnMessage<SendTemplateEmail>(email => { });
        }

        public class SendEmail : ICommand
        {
        }

        public class SendTemplateEmail : ICommand
        {
        }
    }
}