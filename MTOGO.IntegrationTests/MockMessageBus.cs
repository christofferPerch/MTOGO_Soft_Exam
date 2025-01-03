using MTOGO.MessageBus;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTOGO.IntegrationTests.Mocks
{
    public class MockMessageBus : IMessageBus
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _queues = new();
        private readonly ConcurrentDictionary<string, List<Action<object>>> _subscriptions = new();

        public Task PublishMessage(string queueName, string message)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName), "Queue name cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), "Message payload cannot be null or empty.");
            }

            var queue = _queues.GetOrAdd(queueName, _ => new ConcurrentQueue<string>());
            queue.Enqueue(message);

            // Notify subscribers for this queue
            if (_subscriptions.TryGetValue(queueName, out var subscribers))
            {
                foreach (var subscriber in subscribers)
                {
                    try
                    {
                        var deserializedMessage = JsonConvert.DeserializeObject(message, subscriber.GetType().GenericTypeArguments[0]);
                        subscriber.Invoke(deserializedMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error invoking subscriber: {ex.Message}");
                    }
                }
            }

            return Task.CompletedTask;
        }

        public void SubscribeMessage<T>(string queueName, Action<T> onMessageReceived)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName), "Queue name cannot be null or empty.");
            }

            var subscribers = _subscriptions.GetOrAdd(queueName, _ => new List<Action<object>>());
            subscribers.Add(message =>
            {
                if (message is T typedMessage)
                {
                    onMessageReceived?.Invoke(typedMessage);
                }
            });

            // Process any existing messages in the queue
            if (_queues.TryGetValue(queueName, out var queue))
            {
                while (queue.TryDequeue(out var message))
                {
                    try
                    {
                        var deserializedMessage = JsonConvert.DeserializeObject<T>(message);
                        onMessageReceived?.Invoke(deserializedMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                    }
                }
            }
        }
    }
}
