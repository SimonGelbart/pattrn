

using System.Linq;
using System.Threading.Tasks;


// TODO: Make the existing tests pass

namespace Homework.Routing
{
    public class MessageQueueTests
    {
        [Test]
        public async Task ShouldDequeueSingleMessage()
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId = new ClientId("Client.1");
            var message = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId, message);

            // Act
            var result = queue.TryDequeueForClient(clientId, out var dequeue);

            // Assert
            await Assert.That(result).IsTrue();
            await Assert.That(ReferenceEquals(message, dequeue)).IsTrue();
        }

        [Test]
        public async Task ShouldTryDequeueEmptyQueue()
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId = new ClientId("Client.1");

            // Act
            var result = queue.TryDequeueForClient(clientId, out var dequeue);

            // Assert
            await Assert.That(result).IsFalse();
            await Assert.That(dequeue).IsNull();
        }

        [Test]
        public async Task ShouldDequeueSingleMessageForClient()
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId1 = new ClientId("Client.1");
            var message1 = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId1, message1);

            var clientId2 = new ClientId("Client.2");
            var message2 = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId2, message2);

            // Act
            var result = queue.TryDequeueForClient(clientId2, out var dequeue);

            // Assert
            await Assert.That(result).IsTrue();
            await Assert.That(ReferenceEquals(message2, dequeue)).IsTrue();
        }

        [Test]
        public async Task ShouldTryDequeueEmptyQueueForClient()
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId1 = new ClientId("Client.1");
            var message1 = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId1, message1);

            var clientId2 = new ClientId("Client.2");

            // Act
            var result = queue.TryDequeueForClient(clientId2, out var dequeue);

            // Assert
            await Assert.That(result).IsFalse();
            await Assert.That(dequeue).IsNull();
        }

        [Test]
        [Arguments(MessagePriority.Low)]
        [Arguments(MessagePriority.Normal)]
        [Arguments(MessagePriority.High)]
        public async Task ShouldDequeueInOrderForSamePriority(MessagePriority priority)
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId = new ClientId("Client.1");
            var message1 = new RoutableMessages.InstrumentAdded();
            var message2 = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId, message1, priority);
            queue.EnqueueForClient(clientId, message2, priority);

            // Act
            queue.TryDequeueForClient(clientId, out var dequeue1);
            queue.TryDequeueForClient(clientId, out var dequeue2);

            // Assert
            await Assert.That(ReferenceEquals(message1, dequeue1)).IsTrue();
            await Assert.That(ReferenceEquals(message2, dequeue2)).IsTrue();
        }

        [Test]
        public async Task ShouldDequeueHighPriorityMessageFirst()
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId = new ClientId("Client.1");
            var message1 = new RoutableMessages.InstrumentAdded();
            var message2 = new RoutableMessages.InstrumentAdded();
            var message3 = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId, message1, MessagePriority.Low);
            queue.EnqueueForClient(clientId, message2, MessagePriority.Normal);
            queue.EnqueueForClient(clientId, message3, MessagePriority.High);

            // Act
            queue.TryDequeueForClient(clientId, out var dequeue1);
            queue.TryDequeueForClient(clientId, out var dequeue2);
            queue.TryDequeueForClient(clientId, out var dequeue3);

            // Assert
            await Assert.That(ReferenceEquals(message3, dequeue1)).IsTrue();
            await Assert.That(ReferenceEquals(message2, dequeue2)).IsTrue();
            await Assert.That(ReferenceEquals(message1, dequeue3)).IsTrue();
        }

        [Test]
        public async Task ShouldWriteAndReadFromMultipleThreads()
        {
            const int clientCount = 10_00;
            var queue = new MessageQueue();
            var clients = Enumerable.Range(1, clientCount).Select(x => new ClientId($"Client.{x}")).ToArray();

            const int messageCount = 10_000_000;
            Parallel.For(0,
                         messageCount,
                         i => queue.EnqueueForClient(clients[i % clientCount], new RoutableMessages.InstrumentAdded(), (MessagePriority)(i % 3)));

            Parallel.For(0,
                         messageCount,
                         i =>
                         {
                             if (!queue.TryDequeueForClient(clients[i % clientCount], out _))
                             {
                                 throw new InvalidOperationException($"Expected a queued message for iteration {i}.");
                             }
                         });

            // Queue should be empty
            foreach (var clientId in clients)
            {
                await Assert.That(queue.TryDequeueForClient(clientId, out _)).IsFalse();
            }
        }

        [Test]
        public async Task ShouldReadFromMultipleThreads()
        {
            var queue = new MessageQueue();
            var clientId = new ClientId("Client");
            const int messageCount = 1_000_000;
            for (var i = 0; i < messageCount; i++)
            {
                queue.EnqueueForClient(clientId, new SimpleMessages.ExchangeAdded());
            }

            Parallel.For(0, messageCount, i =>
            {
                if (!queue.TryDequeueForClient(clientId, out _))
                {
                    throw new InvalidOperationException($"Expected a queued message for iteration {i}.");
                }
            });

            // Queue should be empty
            await Assert.That(queue.TryDequeueForClient(clientId, out _)).IsFalse();
        }
    }
}
