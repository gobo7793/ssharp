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

using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    /// <summary>
    /// Interface for Hadoop connection
    /// </summary>
    public interface IHadoopConnector
    {

        #region YARN Lists

        /// <summary>
        /// Gets the YARN application list with the given states
        /// </summary>
        /// <param name="states">The states</param>
        /// <returns>The YARN application list</returns>
        string GetYarnApplicationList(string states);

        /// <summary>
        /// Gets the YARN application attempt list for the given app id
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        string GetYarnAppAttemptList(string appId);

        /// <summary>
        /// Gets the YARN application container list for the given application attempt id
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application container list</returns>
        string GetYarnAppContainerList(string attemptId);

        #endregion

        #region YARN Details

        /// <summary>
        /// Gets the YARN application details for the given app id
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application details</returns>
        string GetYarnApplicationDetails(string appId);

        /// <summary>
        /// Gets the YARN application attempt details for the given attempt id
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application attempt details</returns>
        string GetYarnAppAttemptDetails(string attemptId);

        /// <summary>
        /// Gets the YARN application container details for the given container id
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <returns>The YARN application container details</returns>
        string GetYarnAppContainerDetails(string containerId);

        #endregion

        #region YARN Nodes

        /// <summary>
        /// Gets the YARN node list
        /// </summary>
        /// <returns>The YARN node list</returns>
        string GetYarnNodeList();

        /// <summary>
        /// Gets the YARN node details for the given node id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>The YARN node details</returns>
        string GetYarnNodeDetails(string nodeId);

        #endregion

        // TODO: Start benchmark or other things...
    }
}