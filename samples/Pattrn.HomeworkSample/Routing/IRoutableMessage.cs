

namespace Homework.Routing
{
    public interface IRoutableMessage : IMessage
    {
        MessageRoutingContent GetContent();
    }
}
