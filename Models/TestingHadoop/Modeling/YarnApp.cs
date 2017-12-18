using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestingHadoop
{
    /// <summary>
    /// Application/Job to run on the Hadoop cluster
    /// </summary>
    public class YarnApp
    {
        /// <summary>
        /// Starting <see cref="Client"/> of this app
        /// </summary>
        public Client StartingClient
        {
            get => default(Client);
            set
            {
            }
        }

        /// <summary>
        /// Running <see cref="YarnNode"/> for this app
        /// </summary>
        public List<YarnNode> ExecutingNodes
        {
            get => default(List<YarnNode>);
            set
            {
            }
        }

        /// <summary>
        /// Current state
        /// </summary>
        public AppState AppState
        {
            get => default(AppState);
            set
            {
            }
        }
    }
}