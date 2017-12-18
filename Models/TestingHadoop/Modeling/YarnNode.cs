using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestingHadoop
{
    /// <summary>
    /// YARN slave node which executes <see cref="YarnApp"/>s
    /// </summary>
    public class YarnNode
    {
        /// <summary>
        /// <see cref="NodeManager"/> of the node
        /// </summary>
        public NodeManager NodeManager
        {
            get => default(NodeManager);
            set
            {
            }
        }

        /// <summary>
        /// <see cref="YarnApp"/>s executing by this node
        /// </summary>
        public List<YarnApp> ExecutingApps
        {
            get => default(List<YarnApp>);
            set
            {
            }
        }

        /// <summary>
        /// Indicates if node is aktive
        /// </summary>
        public bool IsActive
        {
            get => default(bool);
            set
            {
            }
        }

        /// <summary>
        /// Indicates if the node connection is acitve
        /// </summary>
        public bool IsConnected
        {
            get => default(bool);
            set
            {
            }
        }
    }
}