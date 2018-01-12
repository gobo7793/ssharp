﻿// The MIT License (MIT)
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
}