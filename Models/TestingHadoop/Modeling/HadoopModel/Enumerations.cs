// The MIT License (MIT)
// 
// Copyright (c) 2014-2018, Institute for Software & Systems Engineering
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    #region EAppState

    /// <summary>
    /// <see cref="YarnApp" /> execution states
    /// </summary>
    /// <remarks>
    /// The application state according to the ResourceManager
    ///     - valid values are members of the YarnApplicationState enum:
    ///     NEW, NEW_SAVING, SUBMITTED, ACCEPTED, RUNNING, FINISHED, FAILED, KILLED 
    /// 
    /// via http://hadoop.apache.org/docs/r2.7.1/hadoop-yarn/hadoop-yarn-site/ResourceManagerRest.html#Cluster_Application_API
    /// </remarks>
    [Flags]
    public enum EAppState
    {
        /// <summary>
        /// Default value
        /// </summary>
        None = 0,

        /// <summary>
        /// Job not started yet. State only for use inside model.
        /// Can be used to indicate, that the instance was not in use.
        /// </summary>
        NotStartedYet = 1,

        ALL = 2,
        NEW = 4,
        NEW_SAVING = 8,

        /// <summary>
        /// Job is submitted
        /// </summary>
        SUBMITTED = 16,

        /// <summary>
        /// Job can be executed
        /// </summary>
        ACCEPTED = 32,

        /// <summary>
        /// Job is running
        /// </summary>
        RUNNING = 64,

        /// <summary>
        /// Job is finished
        /// </summary>
        FINISHED = 128,

        /// <summary>
        /// Job execution failed
        /// </summary>
        FAILED = 256,

        /// <summary>
        /// Job was killed while execution
        /// </summary>
        KILLED = 512
    }

    #endregion

    #region EContainerState

    /// <summary>
    /// Status of <see cref="YarnAppContainer"/>
    /// </summary>
    /// <remarks>
    /// State of the container - valid states are:
    ///     NEW, LOCALIZING, LOCALIZATION_FAILED, LOCALIZED, RUNNING,
    ///     EXITED_WITH_SUCCESS, EXITED_WITH_FAILURE, KILLING,
    ///     CONTAINER_CLEANEDUP_AFTER_KILL, CONTAINER_RESOURCES_CLEANINGUP, DONE
    /// TL: COMPLETE
    /// 
    /// via http://hadoop.apache.org/docs/r2.7.1/hadoop-yarn/hadoop-yarn-site/NodeManagerRest.html#Container_API
    /// and http://hadoop.apache.org/docs/r2.7.1/hadoop-yarn/hadoop-yarn-site/TimelineServer.html#GENERIC_DATA_REST_APIS
    /// </remarks>
    public enum EContainerState
    {
        None,
        NEW,
        LOCALIZING,
        LOCALIZATION_FAILED,
        LOCALIZED,
        RUNNING,
        EXITED_WITH_SUCCESS,
        EXITED_WITH_FAILURE,
        KILLING,
        CONTAINER_CLEANEDUP_AFTER_KILL,
        CONTAINER_RESOURCES_CLEANINGUP,
        DONE,
        COMPLETE
    }

    #endregion

    #region EFinalStatus

    /// <summary>
    /// Final status of <see cref="YarnApp"/>
    /// </summary>
    /// <remarks>
    /// The final status of the application if finished
    ///     - reported by the application itself - valid values are:
    ///     UNDEFINED, SUCCEEDED, FAILED, KILLED 
    /// 
    /// via http://hadoop.apache.org/docs/r2.7.1/hadoop-yarn/hadoop-yarn-site/ResourceManagerRest.html#Cluster_Application_API
    /// </remarks>
    public enum EFinalStatus
    {
        /// <summary>
        /// Default value
        /// </summary>
        None = 0,

        /// <summary>
        /// Undefined status
        /// </summary>
        UNDEFINED = 1,

        /// <summary>
        /// Execution successfully finished
        /// </summary>
        SUCCEEDED = 2,

        /// <summary>
        /// Execution failed
        /// </summary>
        FAILED = 4,

        /// <summary>
        /// Execution was killed
        /// </summary>
        KILLED = 8
    }

    #endregion

    #region ENodeState

    /// <summary>
    /// State of <see cref="YarnNode"/>
    /// </summary>
    /// <remarks>
    /// State of the node - valid values are:
    ///     NEW, RUNNING, UNHEALTHY, DECOMMISSIONED, LOST, REBOOTED
    /// 
    /// via http://hadoop.apache.org/docs/r2.7.1/hadoop-yarn/hadoop-yarn-site/ResourceManagerRest.html#Cluster_Node_API
    /// </remarks>
    [Flags]
    public enum ENodeState
    {
        None = 0,
        NEW = 1,
        RUNNING = 2,
        UNHEALTHY = 4,
        DECOMMISSIONED = 8,
        LOST = 16,
        REBOOTED = 32
    }

    #endregion
    
    #region EConstraintType

    /// <summary>
    /// Defines the constraint type to check
    /// </summary>
    public enum EConstraintType
    {
        /// <summary>
        /// Indicates constraints for the system under test
        /// </summary>
        Sut,

        /// <summary>
        /// Indicates constraints for the test suite itself
        /// </summary>
        Test
    }

    #endregion
}