using System.Collections.Generic;
using System.Linq;
using Mandrill.Model;

namespace NServiceBus.Mandrill
{
    public class SendMandrillEmail : IMessage
    {
        public SendMandrillEmail()
        {
            TemplateContents = new List<MandrillTemplateContent>();
        }

        public SendMandrillEmail(MandrillMessage message)
            : this()
        {
            Message = message;
        }

        public SendMandrillEmail(MandrillMessage message, string templateName, IEnumerable<MandrillTemplateContent> templateContents)
            : this()
        {
            Message = message;
            TemplateName = templateName;
            TemplateContents = templateContents.ToList();
        }

        public MandrillMessage Message { get; set; }
        public string TemplateName { get; set; }
        public List<MandrillTemplateContent> TemplateContents { get; set; }

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