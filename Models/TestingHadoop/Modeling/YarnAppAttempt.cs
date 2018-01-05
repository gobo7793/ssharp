using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling
{
    public class YarnAppAttempt
    {
        /// <summary>
        /// YarnApp from this attempt
        /// </summary>
        public YarnApp YarnApp
        {
            get => default(YarnApp);
            set
            {
            }
        }

        /// <summary>
        /// Attempt ID
        /// </summary>
        public string AttemptId
        {
            get => default(int);
            set
            {
            }
        }

        /// <summary>
        /// State
        /// </summary>
        public AppState State
        {
            get => default(AppState);
            set
            {
            }
        }

        /// <summary>
        /// Containers for this Attempt
        /// </summary>
        public System.Collections.Generic.List<YarnAppContainer> Containers
        {
            get => default(int);
            set
            {
            }
        }

        /// <summary>
        /// Container for ApplicationMaster
        /// </summary>
        public YarnAppContainer AmContainer
        {
            get => default(YarnAppContainer);
            set
            {
            }
        }

        /// <summary>
        /// Host for ApplicationMaster
        /// </summary>
        public YarnSlave AmHost
        {
            get => default(YarnSlave);
            set
            {
            }
        }
    }
}