using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestingHadoop
{
    /// <summary>
    /// Master-Node von YARN
    /// </summary>
    public class YarnMaster
    {
        /// <summary>
        /// <see cref="Scheduler"/> des Masters
        /// </summary>
        public Scheduler Scheduler
        {
            get => default(Scheduler);
            set
            {
            }
        }

        /// <summary>
        /// <see cref="ResourceManager"/> des Masters
        /// </summary>
        public ResourceManager ResourceManager
        {
            get => default(ResourceManager);
            set
            {
            }
        }

        /// <summary>
        /// Verbundene <see cref="YarnNode"/>s
        /// </summary>
        public List<YarnNode> ConnectedNodes
        {
            get => default(List<YarnNode>);
            set
            {
            }
        }

        /// <summary>
        /// Findet die einer <see cref="YarnApp"/> zugeordneten <see cref="YarnNode"/> und speichert diese
        /// </summary>
        /// <param name="app">Die App</param>
        public void FindNodesForApp(YarnApp app)
        {
            throw new System.NotImplementedException();
        }
    }
}