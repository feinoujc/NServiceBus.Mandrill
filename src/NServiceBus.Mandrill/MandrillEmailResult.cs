using System;
using Mandrill.Model;

namespace NServiceBus.Mandrill
{
    [Serializable]
    public class MandrillEmailResult
    {
        public MandrillSendMessageResponse Response { get; set; }
    }
}