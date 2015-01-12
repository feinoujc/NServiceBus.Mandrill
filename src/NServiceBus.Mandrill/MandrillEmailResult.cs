using Mandrill.Model;

namespace NServiceBus.Mandrill
{
    public class MandrillEmailResult : IMessage
    {
        public MandrillSendMessageResponse Response { get; set; }
    }
}