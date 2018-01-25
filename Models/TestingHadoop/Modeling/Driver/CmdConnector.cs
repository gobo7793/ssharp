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
using System.Collections.Generic;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    /// <summary>
    /// Hadoop connector for using the cmd line
    /// </summary>
    public class CmdConnector : IHadoopConnector
    {

        #region Properties

        /// <summary>
        /// The monitoring connection
        /// </summary>
        private SshConnection Monitoring { get; }

        /// <summary>
        /// The fault handling connections
        /// </summary>
        private List<SshConnection> Faulting { get; } = new List<SshConnection>();

        /// <summary>
        /// The application submitting connections
        /// </summary>
        private List<SshConnection> Submitting { get; } = new List<SshConnection>();

        /// <summary>
        /// The controller/timeline host to connect
        /// </summary>
        public string Host { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new connection to the hadoop cluster.
        /// Note that one connection is needed for Monitoring, so maybe set <c>/etc/ssh/sshd_config</c>
        /// MaxSessions to the needed value (default 10)!
        /// </summary>
        /// <param name="host">The host name or ip of the cluster</param>
        /// <param name="username">The username for the ssh connections</param>
        /// <param name="privateKeyFilePath">The private key file path</param>
        /// <param name="faultingConnections">The count for fault handling connections</param>
        /// <param name="submittingConnections">The count for submitting application connections</param>
        public CmdConnector(string host, string username, string privateKeyFilePath, int faultingConnections = 2, int submittingConnections = 4)
        {
            Host = host;

            Monitoring = new SshConnection(Host, username, privateKeyFilePath);
            for(int i = 0; i < faultingConnections; i++)
                Faulting.Add(new SshConnection(Host, username, privateKeyFilePath));
            for(int i = 0; i < submittingConnections; i++)
                Submitting.Add(new SshConnection(Host, username, privateKeyFilePath));
        }

        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            Monitoring?.Dispose();
            foreach(var con in Faulting)
                con.Dispose();
            foreach(var con in Submitting)
                con.Dispose();
        }

        #endregion

        #region YARN Lists

        /// <summary>
        /// Gets the YARN application list itself with the given states.
        /// If no states given, hadoop returns all running applications by default.
        /// </summary>
        /// <param name="states">The states</param>
        /// <returns>The YARN application list</returns>
        public string GetYarnApplicationList(string states)
        {
            var cmd = "hdp cmd yarn application -list";
            if(!String.IsNullOrWhiteSpace(states))
                cmd = $"{cmd} -appStates {states}";

            return Monitoring.Run(cmd);
        }

        /// <summary>
        /// Gets the YARN application attempt list itself for the given app id
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        public string GetYarnAppAttemptList(string appId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the YARN application attempt list itself for the given app id from the timeline server
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        public string GetYarnAppAttemptListTl(string appId)
        {
            return GetYarnAppAttemptList(appId);
        }

        /// <summary>
        /// Gets the YARN application container list itself for the given id of application attempt or node http url
        /// </summary>
        /// <param name="id">The attempt or node id</param>
        /// <returns>The YARN application container list</returns>
        public string GetYarnAppContainerList(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the YARN application container list itself for the given application attempt id from the timeline server
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application container list</returns>
        public string GetYarnAppContainerListTl(string attemptId)
        {
            return GetYarnAppContainerList(attemptId);
        }

        #endregion

        #region YARN Details
        /// <summary>
        /// Gets the YARN application details itself for the given app id
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application details</returns>
        public string GetYarnApplicationDetails(string appId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the YARN application attempt details itself for the given attempt id
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application attempt details</returns>
        public string GetYarnAppAttemptDetails(string attemptId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the YARN application attempt details itself for the given attempt id from the timeline server
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application attempt details</returns>
        public string GetYarnAppAttemptDetailsTl(string attemptId)
        {
            return GetYarnAppContainerDetailsTl(attemptId);
        }

        /// <summary>
        /// Gets the YARN application container details itself for the given container id
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <returns>The YARN application container details</returns>
        public string GetYarnAppContainerDetails(string containerId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the YARN application container details itself for the given container id from the timeline server
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <returns>The YARN application container details</returns>
        public string GetYarnAppContainerDetailsTl(string containerId)
        {
            return GetYarnAppContainerDetails(containerId);
        }

        #endregion

        #region YARN Nodes
        /// <summary>
        /// Gets the YARN node list itself
        /// </summary>
        /// <returns>The YARN node list</returns>
        public string GetYarnNodeList()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the YARN node details itself for the given node id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>The YARN node details</returns>
        public string GetYarnNodeDetails(string nodeId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts the docker container for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if YARN Node started successfully</returns>
        public bool StartNode(string nodeName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stops the docker container for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if VM stopped successfully</returns>
        public bool StopNode(string nodeName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts the docker container network connection for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if network connection started successfully</returns>
        public bool StartNodeNetConnection(string nodeName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stops the docker container network connection for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if network connection stopped successfully</returns>
        public bool StopNodeNetConnection(string nodeName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Application Control

        /// <summary>
        /// Kills the given application and returns true if no errors occurs
        /// </summary>
        /// <param name="appId">The app id for the application</param>
        /// <returns>True if application killed</returns>
        public bool KillApplication(string appId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Submits the given application with the given arguments to Hadoop
        /// </summary>
        /// <param name="name">The application to submit</param>
        /// <param name="arguments">The arguments</param>
        public void StartApplication(string name, params string[] arguments)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}