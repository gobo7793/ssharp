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

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    /// <summary>
    /// Interface for Hadoop connection
    /// </summary>
    public interface IHadoopConnector : IDisposable
    {

        #region YARN Lists

        /// <summary>
        /// Gets the YARN application list itself with the given states
        /// </summary>
        /// <param name="states">The states</param>
        /// <returns>The YARN application list</returns>
        string GetYarnApplicationList(string states);

        /// <summary>
        /// Gets the YARN application attempt list itself for the given app id
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        string GetYarnAppAttemptList(string appId);

        /// <summary>
        /// Gets the YARN application attempt list itself for the given app id from the timeline server
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        string GetYarnAppAttemptListTl(string appId);

        /// <summary>
        /// Gets the YARN application container list itself for the given id of application attempt or node http url
        /// </summary>
        /// <param name="id">The attempt or node id</param>
        /// <returns>The YARN application container list</returns>
        string GetYarnAppContainerList(string id);

        /// <summary>
        /// Gets the YARN application container list itself for the given application attempt id from the timeline server
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application container list</returns>
        string GetYarnAppContainerListTl(string attemptId);

        #endregion

        #region YARN Details

        /// <summary>
        /// Gets the YARN application details itself for the given app id
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application details</returns>
        string GetYarnApplicationDetails(string appId);

        /// <summary>
        /// Gets the YARN application attempt details itself for the given attempt id
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application attempt details</returns>
        string GetYarnAppAttemptDetails(string attemptId);

        /// <summary>
        /// Gets the YARN application attempt details itself for the given attempt id from the timeline server
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application attempt details</returns>
        string GetYarnAppAttemptDetailsTl(string attemptId);

        /// <summary>
        /// Gets the YARN application container details itself for the given container id
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <param name="nodeUrl">The node url from the container</param>
        /// <returns>The YARN application container details</returns>
        string GetYarnAppContainerDetails(string containerId, string nodeUrl = null);

        /// <summary>
        /// Gets the YARN application container details itself for the given container id from the timeline server
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <returns>The YARN application container details</returns>
        string GetYarnAppContainerDetailsTl(string containerId);

        #endregion

        #region YARN Nodes

        /// <summary>
        /// Gets the YARN node list itself
        /// </summary>
        /// <returns>The YARN node list</returns>
        string GetYarnNodeList();

        /// <summary>
        /// Gets the YARN node details itself for the given node id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>The YARN node details</returns>
        string GetYarnNodeDetails(string nodeId);

        /// <summary>
        /// Starts the docker container for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if YARN Node started successfully</returns>
        bool StartNode(string nodeName);

        /// <summary>
        /// Stops the docker container for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if VM stopped successfully</returns>
        bool StopNode(string nodeName);

        /// <summary>
        /// Starts the docker container network connection for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if network connection started successfully</returns>
        bool StartNodeNetConnection(string nodeName);

        /// <summary>
        /// Stops the docker container network connection for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if network connection stopped successfully</returns>
        bool StopNodeNetConnection(string nodeName);

        #endregion

        #region Application Control

        /// <summary>
        /// Kills the given application and returns true if no errors occurs
        /// </summary>
        /// <param name="appId">The app id for the application</param>
        /// <returns>True if application killed</returns>
        bool KillApplication(string appId);

        /// <summary>
        /// Submits the given application with the given arguments to Hadoop,
        /// waits for end of execution and returns it application id.
        /// If no application id found all output will be returned.
        /// </summary>
        /// <param name="cmd">The command to submit</param>
        /// <returns>The application id for the submitted app</returns>
        string StartApplication(string cmd);

        /// <summary>
        /// Submits the given application with the given arguments to Hadoop,
        /// waits for the application id and returns it immediately.
        /// If no application id found all output will be returned.
        /// </summary>
        /// <param name="cmd">The command to submit</param>
        /// <returns>The application id for the submitted app</returns>
        string StartApplicationAsyncTillId(string cmd);

        /// <summary>
        /// Submits the given application with the given arguments to Hadoop.
        /// The application will be fully executed async with no return values.
        /// </summary>
        /// <param name="cmd">The command to submit</param>
        void StartApplicationAsyncFull(string cmd);

        /// <summary>
        /// Checks if the given directory exists on hdfs
        /// </summary>
        /// <param name="directory">The directory to check</param>
        /// <returns>True if the directory exists</returns>
        bool ExistsHdfsDir(string directory);

        /// <summary>
        /// Removes the given directory on hdfs
        /// </summary>
        /// <param name="directory">The directory to remove</param>
        void RemoveHdfsDir(string directory);

        /// <summary>
        /// Returns the version information from Hadoop
        /// </summary>
        /// <returns>Hadoop version</returns>
        string GetHadoopVersion();

        #endregion
    }
}