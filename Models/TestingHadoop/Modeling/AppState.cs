using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestingHadoop
{
    /// <summary>
    ///   <see cref="YarnApp" /> execution states
    /// </summary>
    /// <remarks>
    /// The valid application state can be one of the following:
    ///   ALL, NEW, NEW_SAVING, SUBMITTED, ACCEPTED, RUNNING,
    ///   FINISHED, FAILED, KILLED
    /// 
    /// via http://hadoop.apache.org/docs/r2.7.5/hadoop-yarn/hadoop-yarn-site/YarnCommands.html
    /// </remarks>
    public enum AppState
    {
        ALL,
        NEW,
        NEW_SAVING,

        /// <summary>
        /// Job is submitted
        /// </summary>
        SUBMITTED,

        /// <summary>
        /// Job can be executed
        /// </summary>
        ACCEPTED,

        /// <summary>
        /// Job is running
        /// </summary>
        RUNNING,

        /// <summary>
        /// Job is finished
        /// </summary>
        FINISHED,

        /// <summary>
        /// Job execution failed
        /// </summary>
        FAILED,

        /// <summary>
        /// Job was killed while execution
        /// </summary>
        KILLED
    }
}