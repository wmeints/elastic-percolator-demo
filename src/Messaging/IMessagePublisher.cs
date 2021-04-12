using System;
using System.Threading.Tasks;

namespace Messaging
{
    /// <summary>
    /// Use this interface to publish messages in the application
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publishes a message to a Kafka topic
        /// </summary>
        /// <param name="topic">Topic to publish to</param>
        /// <param name="payload">Payload of the message</param>
        /// <returns>Returns the ID of the message</returns>
        Task PublishAsync(string topic, object payload);
    }
}