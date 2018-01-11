// The MIT License (MIT)
// 
// Copyright (c) 2014-2016, Institute for Software & Systems Engineering
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
    /// Final status of <see cref="YarnApp"/>
    /// </summary>
    /// <remarks>
    /// The final status of the application if finished
    ///     - reported by the application itself - valid values are:
    ///     UNDEFINED, SUCCEEDED, FAILED, KILLED 
    /// 
    /// via http://hadoop.apache.org/docs/r2.7.1/hadoop-yarn/hadoop-yarn-site/ResourceManagerRest.html#Cluster_Application_API
    /// </remarks>
    [Flags]
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
}