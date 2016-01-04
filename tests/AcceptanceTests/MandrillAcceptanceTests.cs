using System;
using System.Collections.Generic;
using System.Linq;
using Mandrill;
using Mandrill.Model;
using NServiceBus;
using NServiceBus.AcceptanceTesting;
using NServiceBus.AcceptanceTests.EndpointTemplates;
using NServiceBus.AcceptanceTests.ScenarioDescriptors;
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
                .WithEndpoint<MandrillEndpointWithReply>(builder => builder
                    .Given(bus =>
                    {
                        bus.SendEmail(new MandrillMessage("nservicebus@example.com", "recipient@mandrill.com",
                            "nservicebus test", "did you get it?"));
                    }))
                .Done(c => c.Sent.Count == 1
                           && c.Sent[0].GetMessage().FromEmail == "nservicebus@example.com"
                           && c.Replies.Count == 0)
                .Run();
        }

        [Test]
        public void When_sending_email_message_message_is_sent_through_mandrill_satellite_with_handled_reply()
        {
            Scenario.Define<Context>()
                .WithEndpoint<MandrillEndpointWithReply>(builder => builder
                    .Given(bus =>
                    {
                        bus.SendEmail(new MandrillMessage("nservicebus@example.com", "recipient@mandrill.com",
                            "nservicebus test", "did you get it?"));
                    }))
                .Done(c => c.Replies.Count == 1
                           && c.Replies[0].Response.Email == "recipient@mandrill.com"
                           && (c.Replies[0].Response.Status == MandrillSendMessageResponseStatus.Sent ||
                               c.Replies[0].Response.Status == MandrillSendMessageResponseStatus.Queued))
                .Run();
        }

        [Test]
        public void When_sending_complex_template_content_using_xml_serializer_there_are_no_serialization_issues()
        {
            var api = new MandrillApi(Environment.GetEnvironmentVariable("MANDRILL_API_KEY"));
            
            var templateName = $"acceptance-test-{Guid.NewGuid():N}";

            var from = $"{Guid.NewGuid():N}@mandrill.com";
            var fromName = $"Acceptance Test";
            var subject = "Test acceptance test template";
            api.Templates.Add(templateName, HandleBarCode, "", true, from, fromName, subject);

            var message = new MandrillMessage
            {
                MergeLanguage = MandrillMessageMergeLanguage.Handlebars,
                To = new List<MandrillMailAddress>()
                {
                    new MandrillMailAddress("test1@example.com", "Test1 User"),
                    new MandrillMailAddress("test2@example.com", "Test2 User")
                }
            };
            message.AddGlobalMergeVars("ORDERDATE", DateTime.UtcNow.ToString("d"));
            var products = new List<ProductEmailModel>
            {
                new ProductEmailModel
                {
                    Name = "Bolts",
                    Description = "1/4 \" bolts",
                    Price = 0.99,
                    Qty = 20,
                    Sku = "APL",
                    OrdPrice = 0.99*20
                },
                new ProductEmailModel
                {
                    Name = "Screws",
                    Description = "5/8 \" screws",
                    Price = 1.20,
                    Qty = 80,
                    Sku = "APL",
                    OrdPrice = 1.20*80
                }
            };

            message.AddGlobalMergeVars("products", products);
            message.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;

            try
            {
                Scenario.Define<Context>()
                    .WithEndpoint<MandrillEndpointWithReply>(builder => builder
                        .CustomConfig(configuration => configuration.UseSerialization<XmlSerializer>())
                        .Given(bus =>
                        {
                            bus.SendEmailTemplate(message, templateName);
                        }))
                        
                    .Done(c => c.Sent.Count == 1
                               && c.Sent[0].TemplateName == templateName
                               && c.Replies.Count == 2
                               && c.Replies.Count(r => r.Response.Email == "test1@example.com") == 1
                               && c.Replies.Count(r => r.Response.Email == "test2@example.com") == 1
                               && (c.Replies.All(r => r.Response.Status == MandrillSendMessageResponseStatus.Sent ||
                                                      c.Replies[0].Response.Status == MandrillSendMessageResponseStatus.Queued)))
                    .Run();
            }
            finally
            {
                try
                {
                    api.Templates.Delete(templateName);
                }
                catch (MandrillException)
                {
                    //ignore
                }
            }
        }


        public class ReplyHandler : IHandleMessages<MandrillEmailResult>
        {
            public Context Context { get; set; }

            public void Handle(MandrillEmailResult message)
            {
                Context.Replies.Add(message);
            }
        }


        public class ProductEmailModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public double Price { get; set; }
            public string Sku { get; set; }
            public int Qty { get; set; }
            public double OrdPrice { get; set; }
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

        public const string HandleBarCode = @"<html>
<head>
	<title>a test</title>
</head>
<body>
	
	<p>Dear{{fname}},</p>
   <p>Thank you for your purchase on {{orderdate}} from ABC Widget Company. <br>
We appreciate your business and have included a copy of your invoice below. <br>
   <!-- BEGIN PRODUCT LOOP // -->
   {{#each products}}
   <tr class=""item"">
        <td valign=""top"" class=""textContent"">
            <h4 class=""itemName"">{{name}}</h4>
            <span class=""contentSecondary"">Qty: {{qty}} x ${{price}}/each</span><br />
            <span class=""contentSecondary sku""><em>{{sku}}</em></span><br />
            <span class=""contentSecondary itemDescription"">{{description}}</span>
        </td>
        <td valign=""top"" class=""textContent alignRight priceWidth"">
            ${{ord_price}}
        </td>
    </tr>
    {{/each}}
<!-- // END PRODUCT LOOP -->
Please let us know if you have further questions.


     -- ABC Widget Co.</p>

     <div mc:edit=""footer"">footer</div>
</body>
</html>";
    }
}