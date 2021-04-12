using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging
{
    /// <summary>
    /// Implement this interface to process a message.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMessageHandler<TMessage>
    {
        /// <summary>
        /// Handles a message received from Kafka
        /// </summary>
        /// <param name="messageBody">Deserialized message content</param>
        /// <returns>Returns an awaitable task</returns>
        Task HandleAsync(TMessage messageBody);
    }
}
