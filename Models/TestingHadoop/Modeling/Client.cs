using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestingHadoop
{
    /// <summary>
    /// Client, der auf Hadoop zugreift
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Gestartete <see cref="YarnApp"/>s des Clients
        /// </summary>
        public List<YarnApp> StartedYarnApps
        {
            get => default(List<YarnApp>);
            set
            {
            }
        }

        /// <summary>
        /// Verbundener <see cref="YarnMaster"/>
        /// </summary>
        public YarnMaster ConnectedYarnMaster
        {
            get => default(YarnMaster);
            set
            {
            }
        }

        /// <summary>
        /// Startet den angegebenen Job auf dem <see cref="ConnectedYarnMaster"/>
        /// </summary>
        /// <param name="app">Zu startender Job</param>
        public void StartJob(YarnApp app)
        {
            throw new System.NotImplementedException();
        }
    }
}