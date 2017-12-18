using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestingHadoop
{
    /// <summary>
    /// Anwendung, die auf dem Hadoop-System ausgeführt werden soll
    /// </summary>
    public class YarnApp
    {
        /// <summary>
        /// <see cref="Client"/>, der die App gestartet hat
        /// </summary>
        public Client StartingClient
        {
            get => default(Client);
            set
            {
            }
        }

        /// <summary>
        /// <see cref="YarnNode"/>s, auf denen die Apps ausgeführt wird
        /// </summary>
        public List<YarnNode> ExecutingNodes
        {
            get => default(List<YarnNode>);
            set
            {
            }
        }
    }
}