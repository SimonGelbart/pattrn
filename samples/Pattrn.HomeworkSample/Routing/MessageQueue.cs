

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Homework.Routing
{
    public class MessageQueue
    {
        private readonly ConcurrentDictionary<ClientId, ConcurrentQueue<IMessage>> _lowPriorityQueues = new ();
        private readonly ConcurrentDictionary<ClientId, ConcurrentQueue<IMessage>> _normalPriorityQueues = new();
        private readonly ConcurrentDictionary<ClientId, ConcurrentQueue<IMessage>> _highPriorityQueues = new ();


        public void EnqueueForClient(ClientId clientId, IMessage message,
            MessagePriority priority = MessagePriority.Normal)
        {
            var queue = GetQueueByPriority(priority, clientId);
            queue.Enqueue(message);
        }

        public bool TryDequeueForClient(ClientId clientId, out IMessage? message)
        {
            if (TryDequeueMessage(_highPriorityQueues, clientId, out message))
            {
                return true;
            }

            if (TryDequeueMessage(_normalPriorityQueues, clientId, out message))
            {
                return true;
            }

            if (TryDequeueMessage(_lowPriorityQueues, clientId, out message))
            {
                return true;
            }

            return false;
        }

        private bool TryDequeueMessage(ConcurrentDictionary<ClientId, ConcurrentQueue<IMessage>> queues,
            ClientId clientId, out IMessage? message)
        {
            if (queues.TryGetValue(clientId, out var queue) && queue.TryDequeue(out message))
            {
                if (queue.IsEmpty)
                {
                    queues.TryRemove(clientId, out _);
                }

                return true;
            }

            message = null;
            return false;
        }

        private ConcurrentQueue<IMessage> GetQueueByPriority(MessagePriority priority, ClientId clientId)
        {
            return priority switch
            {
                MessagePriority.Low => _lowPriorityQueues.GetOrAdd(clientId, _ => new ConcurrentQueue<IMessage>()),
                MessagePriority.Normal => _normalPriorityQueues.GetOrAdd(clientId, _ => new ConcurrentQueue<IMessage>()),
                MessagePriority.High => _highPriorityQueues.GetOrAdd(clientId, _ => new ConcurrentQueue<IMessage>()),
                _ => throw new ArgumentOutOfRangeException(nameof(priority))
            };
        }
    }
}
