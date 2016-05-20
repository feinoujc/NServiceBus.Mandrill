using NServiceBus;

namespace Sample
{
    internal class GetMailContent : ICommand
    {
        public string MessageId { get; set; }
    }
}