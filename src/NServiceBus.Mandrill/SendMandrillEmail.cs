using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mandrill.Model;
using Mandrill.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NServiceBus.Mandrill
{
    public class SendMandrillEmail
    {
        protected internal SendMandrillEmail()
        {
            TemplateContents = new List<MandrillTemplateContent>();
        }

        protected internal SendMandrillEmail(MandrillMessage message)
            : this()
        {
            MessageBody = SerializeMessageBody(message);
        }

        protected internal SendMandrillEmail(MandrillMessage message, string templateName, IEnumerable<MandrillTemplateContent> templateContents)
            : this()
        {
            MessageBody = SerializeMessageBody(message);
            TemplateName = templateName;
            TemplateContents = templateContents != null ? templateContents.ToList() : null;
        }

        public string MessageBody { get; private set; }
        public string TemplateName { get; private set; }
        public List<MandrillTemplateContent> TemplateContents { get; private set; }

        private string SerializeMessageBody(MandrillMessage message)
        {
            //to get around limitations of the nsb serializers, convert to json first
            var sb = new StringBuilder();
            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                MandrillSerializer.Instance.Serialize(writer, message);
                writer.Flush();
            }
            return sb.ToString();
        }

        public MandrillMessage GetMessage()
        {
            using (var reader = new JsonTextReader(new StringReader(MessageBody)))
            {
                return MandrillSerializer.Instance.Deserialize<MandrillMessage>(reader);
            }
        }

        public void AddTemplateContent(string name, string content)
        {
            if (TemplateContents == null)
            {
                TemplateContents = new List<MandrillTemplateContent>();
            }

            TemplateContents.Add(new MandrillTemplateContent {Name = name, Content = content});
        }
    }
}