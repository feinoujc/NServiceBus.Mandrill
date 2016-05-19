using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Mandrill.Model;
using Mandrill.Serialization;
using Newtonsoft.Json;

namespace NServiceBus.Mandrill
{
    [Serializable]
    public class SendMandrillEmail
    {
        protected internal SendMandrillEmail()
        {
        }

        protected internal SendMandrillEmail(MandrillMessage message)
        {
            MessageBody = SerializeMessageBody(message);
        }

        protected internal SendMandrillEmail(MandrillMessage message, string templateName, IEnumerable<MandrillTemplateContent> templateContents)
            : this()
        {
            MessageBody = SerializeMessageBody(message);
            TemplateName = templateName;
            TemplateContents = templateContents?.ToList();
        }

        public string MessageBody { get; set; }
        public string TemplateName { get; set; }
        public List<MandrillTemplateContent> TemplateContents { get; set; }

        private string SerializeMessageBody(MandrillMessage message)
        {
            //to get around limitations of the nsb serializers, convert to json first
            var sb = new StringBuilder();
            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                MandrillSerializer<MandrillMessage>.Serialize(writer, message);
                writer.Flush();
            }
            return sb.ToString();
        }

        public MandrillMessage GetMessage()
        {
            using (var reader = new JsonTextReader(new StringReader(MessageBody)))
            {
                return MandrillSerializer<MandrillMessage>.Deserialize(reader);
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