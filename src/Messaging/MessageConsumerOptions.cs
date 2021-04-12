using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging
{
    public class MessageConsumerOptions
    {
        /// <summary>
        /// Gets or sets the list of bootstrap servers to connect to. 
        /// </summary>
        /// <value>Semi-colon separated list of port + server combinations. e.g.: localhost:9092;otherhost:9092</value>
        public string Servers { get; set; }

        /// <summary>
        /// Gets or sets the topic to subscribe to
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets the application ID for the broker connection.
        /// </summary>
        public string ApplicationId { get; set; }
    }
}
