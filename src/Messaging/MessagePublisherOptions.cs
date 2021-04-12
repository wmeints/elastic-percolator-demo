using System;
using System.Linq;

namespace Messaging
{
    /// <summary>
    /// Defines the options for the message publisher
    /// </summary>
    public class MessagePublisherOptions
    {
        /// <summary>
        /// Gets or sets the list of bootstrap servers to connect to. 
        /// </summary>
        /// <value>Semi-colon separated list of port + server combinations. e.g.: localhost:9092;otherhost:9092</value>
        public string Servers { get; set; }

        /// <summary>
        /// Gets or sets the application ID for the broker connection.
        /// </summary>
        public string ApplicationId { get; set; }
    }
}
