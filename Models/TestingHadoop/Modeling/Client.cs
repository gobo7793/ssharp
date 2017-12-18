using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestingHadoop
{
    /// <summary>
    /// Client for the Hadoop cluster
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Started <see cref="YarnApp"/>s of the client
        /// </summary>
        public List<YarnApp> StartedYarnApps
        {
            get => default(List<YarnApp>);
            set
            {
            }
        }

        /// <summary>
        /// Connected <see cref="YarnMaster"/> for the client
        /// </summary>
        public YarnMaster ConnectedYarnMaster
        {
            get => default(YarnMaster);
            set
            {
            }
        }

        /// <summary>
        /// Starts the given <see cref="YarnApp"/> on <see cref="ConnectedYarnMaster" />
        /// </summary>
        /// <param name="app"><see cref="YarnApp"/> to start</param>
        public void StartJob(YarnApp app)
        {
            throw new System.NotImplementedException();
        }
    }
}