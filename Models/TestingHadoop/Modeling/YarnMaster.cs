using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestingHadoop
{
    /// <summary>
    /// YARN Master node
    /// </summary>
    public class YarnMaster
    {
        /// <summary>
        /// YARN <see cref="Scheduler"/>
        /// </summary>
        public Scheduler Scheduler
        {
            get => default(Scheduler);
            set
            {
            }
        }

        /// <summary>
        /// YARN <see cref="ResourceManager"/>
        /// </summary>
        public ResourceManager ResourceManager
        {
            get => default(ResourceManager);
            set
            {
            }
        }

        /// <summary>
        /// Connected YARN Slaves
        /// </summary>
        public List<YarnNode> ConnectedNodes
        {
            get => default(List<YarnNode>);
            set
            {
            }
        }

        /// <summary>
        /// Indicates the <see cref="YarnNode"/> which executes the given <see cref="YarnApp"/> and saves it
        /// </summary>
        /// <param name="app">The app/job</param>
        public void FindNodesForApp(YarnApp app)
        {
            throw new System.NotImplementedException();
        }
    }
}