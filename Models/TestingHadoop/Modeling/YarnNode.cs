using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestingHadoop
{
    /// <summary>
    /// Slave-Node von YARN, auf dem <see cref="YarnApp"/>s ausgeführt werden
    /// </summary>
    public class YarnNode
    {
        /// <summary>
        /// Zugeordneter <see cref="NodeManager"/>
        /// </summary>
        public NodeManager NodeManager
        {
            get => default(NodeManager);
            set
            {
            }
        }

        /// <summary>
        /// Auf Node ausgeführte <see cref="YarnApp"/>s
        /// </summary>
        public List<YarnApp> ExecutingApps
        {
            get => default(List<YarnApp>);
            set
            {
            }
        }
    }
}