using System;
using System.Collections.Generic;
using Mandrill.Model;
using NServiceBus;
using NServiceBus.AcceptanceTesting;
using NServiceBus.AcceptanceTests.EndpointTemplates;
using NServiceBus.Mandrill;
using NServiceBus.MessageMutator;
using NUnit.Framework;

namespace AcceptanceTests
{
    [TestFixture]
    public class MandrillAcceptanceTests
    {
        [Test]
        public void When_sending_email_message_message_is_sent_through_mandrill_satellite_with_no_reply()
        {
            Scenario.Define<Context>()
                .WithEndpoint<MandrillEndpointWithNoReply>(
                    builder =>
                    {
                        builder.Given(b =>
                            b.SendEmail(new MandrillMessage("nservicebus@example.com", "recipient@mandrill.com",
                                "nservicebus test", "did you get it?")));
                    })
                .Done(c => c.Sent.Count == 1
                           && c.Sent[0].GetMessage().FromEmail == "nservicebus@example.com"
                           && c.Replies.Count == 0)
                .Run();
        }

        [Test]
        public void When_sending_email_message_message_is_sent_through_mandrill_satellite_with_handled_reply()
        {
            Scenario.Define<Context>()
                .WithEndpoint<MandrillEndpointWithReply>(
                    builder =>
                    {
                        builder.Given(b =>
                            b.SendEmail(new MandrillMessage("nservicebus@example.com", "recipient@mandrill.com",
                                "nservicebus test", "did you get it?")));
                    })
                .Done(c => c.Replies.Count == 1
                           && c.Replies[0].Response.Email == "recipient@mandrill.com"
                           && (c.Replies[0].Response.Status == MandrillSendMessageResponseStatus.Sent ||
                               c.Replies[0].Response.Status == MandrillSendMessageResponseStatus.Queued))
                .Run();
        }


        public class ReplyHandler : IHandleMessages<MandrillEmailResult>
        {
            public Context Context { get; set; }

            public void Handle(MandrillEmailResult message)
            {
                Context.Replies.Add(message);
            }
        }


        public class Context : ScenarioContext
        {
            public Context()
            {
                Sent = new List<SendMandrillEmail>();
                Replies = new List<MandrillEmailResult>();
            }

            public bool MandrillEmailSent => Sent.Count > 0;
            public List<SendMandrillEmail> Sent { get; set; }
            public List<MandrillEmailResult> Replies { get; set; }
        }


        public abstract class MandrillEndpoint : EndpointConfigurationBuilder
        {
            public MandrillEndpoint()
            {
                EndpointSetup<DefaultServer>(EnableMandrill);
                CustomEndpointName("mandrill-acceptance-tests");
            }

            protected abstract bool ReplyResult { get; }

            protected void EnableMandrill(BusConfiguration configuration)
            {
                var apiKey = Environment.GetEnvironmentVariable("MANDRILL_API_KEY");
                if (string.IsNullOrEmpty(apiKey))
                    throw new InvalidOperationException("MANDRILL_API_KEY not found in environment variable!");

                configuration.UseMandrill(apiKey, ReplyResult);
            }
        }

        public class MandrillEndpointWithNoReply : MandrillEndpoint
        {
            protected override bool ReplyResult => false;
        }

        public class MandrillEndpointWithReply : MandrillEndpoint
        {
            protected override bool ReplyResult => true;
        }

        private class MandrillContextInspector : IMutateOutgoingMessages, INeedInitialization
        {
            // Will be injected via DI
            public Context TestContext { get; set; }

            public object MutateOutgoing(object message)
            {
                var item = message as SendMandrillEmail;
                if (item != null)
                {
                    TestContext.Sent.Add(item);
                }

                return message;
            }

            public void Customize(BusConfiguration configuration)
            {
                configuration.RegisterComponents(
                    c => c.ConfigureComponent<MandrillContextInspector>(DependencyLifecycle.InstancePerCall));
            }
        }
    }
}