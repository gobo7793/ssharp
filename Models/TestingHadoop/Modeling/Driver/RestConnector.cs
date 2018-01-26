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
    /// Hadoop connector for using the REST API
    /// </summary>
    public class RestConnector : IHadoopConnector
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
        /// ResourceManager URL
        /// </summary>
        private string RmUrl { get; }

        /// <summary>
        /// Timeline server URL
        /// </summary>
        private string TlUrl { get; }

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
        /// <param name="rmUrl">HTTP URL of ResourceManager</param>
        /// <param name="tlUrl">HTTP URL of Timeline server</param>
        /// <param name="forMonitoring">True to use this connection for monitoring</param>
        /// <param name="faultingConnections">The count for fault handling connections</param>
        /// <param name="submittingConnections">The count for submitting application connections</param>
        public RestConnector(string host, string username, string privateKeyFilePath, string rmUrl, string tlUrl,
                             bool forMonitoring = true, int faultingConnections = 2, int submittingConnections = 4)
        {
            RmUrl = rmUrl;
            TlUrl = tlUrl;

            if(forMonitoring)
                Monitoring = new SshConnection(host, username, privateKeyFilePath);
            for(int i = 0; i < faultingConnections; i++)
                Faulting.Add(new SshConnection(host, username, privateKeyFilePath));
            for(int i = 0; i < submittingConnections; i++)
                Submitting.Add(new SshConnection(host, username, privateKeyFilePath));
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
        /// If no states given, Hadoop returns all applications by default.
        /// </summary>
        /// <param name="states">The states</param>
        /// <returns>The YARN application list</returns>
        public string GetYarnApplicationList(string states)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(RestConnector)} not for monitoring initialized!");

            var cmd = $"curl {RmUrl}/ws/v1/cluster/apps";
            if(!String.IsNullOrWhiteSpace(states))
                cmd = $"{cmd}?states={states}";

            return Monitoring.Run(cmd);
        }

        /// <summary>
        /// Gets the YARN application attempt list itself for the given app id
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        public string GetYarnAppAttemptList(string appId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(RestConnector)} not for monitoring initialized!");

            return Monitoring.Run($"curl {RmUrl}/ws/v1/cluster/apps/{appId}/appattempts");
        }

        /// <summary>
        /// Gets the YARN application attempt list itself for the given app id from the timeline server
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        public string GetYarnAppAttemptListTl(string appId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(RestConnector)} not for monitoring initialized!");

            return Monitoring.Run($"curl {TlUrl}/ws/v1/applicationhistory/apps/{appId}/appattempts");
        }

        /// <summary>
        /// Gets the YARN application container list itself for the given node http url
        /// </summary>
        /// <param name="nodeUrl">The attempt or node id</param>
        /// <returns>The YARN application container list</returns>
        public string GetYarnAppContainerList(string nodeUrl)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(RestConnector)} not for monitoring initialized!");

            return Monitoring.Run($"curl {nodeUrl}/ws/v1/node/containers");
        }

        /// <summary>
        /// Gets the YARN application container list itself for the given application attempt id from the timeline server
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application container list</returns>
        public string GetYarnAppContainerListTl(string attemptId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(RestConnector)} not for monitoring initialized!");

            var appId = ParserUtilities.BuildAppIdFromAttempt(attemptId);

            return Monitoring.Run($"curl {TlUrl}/ws/v1/applicationhistory/apps/{appId}/appattempts/{attemptId}/containers");
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
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(RestConnector)} not for monitoring initialized!");

            return Monitoring.Run($"curl {RmUrl}/ws/v1/cluster/apps/{appId}");
        }

        /// <summary>
        /// Gets the YARN application attempt details itself for the given attempt id
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application attempt details</returns>
        public string GetYarnAppAttemptDetails(string attemptId)
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not return any application attempt details. " +
                                                    $"Use {nameof(GetYarnAppAttemptList)} and filter for needed attempt!");
        }

        /// <summary>
        /// Gets the YARN application attempt details itself for the given attempt id from the timeline server
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application attempt details</returns>
        public string GetYarnAppAttemptDetailsTl(string attemptId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(RestConnector)} not for monitoring initialized!");

            var appId = ParserUtilities.BuildAppIdFromAttempt(attemptId);

            return Monitoring.Run($"curl {TlUrl}/ws/v1/applicationhistory/apps/{appId}/appattempts/{attemptId}");
        }

        /// <summary>
        /// Gets the YARN application container details itself for the given container id from the given node
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <param name="nodeUrl">Node URL of the container</param>
        /// <returns>The YARN application container details</returns>
        public string GetYarnAppContainerDetails(string containerId, string nodeUrl = null)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(RestConnector)} not for monitoring initialized!");
            if(nodeUrl == null)
                throw new ArgumentNullException($"No node url given to get details for container {containerId}!");

            return Monitoring.Run($"curl {nodeUrl}/ws/v1/node/containers/{containerId}");
        }

        /// <summary>
        /// Gets the YARN application container details itself for the given container id from the timeline server
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <returns>The YARN application container details</returns>
        public string GetYarnAppContainerDetailsTl(string containerId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(RestConnector)} not for monitoring initialized!");

            var attemptId = ParserUtilities.BuildAttemptIdFromContainer(containerId);
            var appId = ParserUtilities.BuildAppIdFromAttempt(attemptId);

            return Monitoring.Run($"curl {TlUrl}/ws/v1/applicationhistory/apps/{appId}/appattempts/{attemptId}/containers/{containerId}");
        }

        #endregion

        #region YARN Nodes
        /// <summary>
        /// Gets the YARN node list itself
        /// </summary>
        /// <returns>The YARN node list</returns>
        public string GetYarnNodeList()
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(RestConnector)} not for monitoring initialized!");

            return Monitoring.Run($"curl {RmUrl}/ws/v1/cluster/nodes");
        }

        /// <summary>
        /// Gets the YARN node details itself for the given node id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>The YARN node details</returns>
        public string GetYarnNodeDetails(string nodeId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(RestConnector)} not for monitoring initialized!");

            return Monitoring.Run($"curl {RmUrl}/ws/v1/cluster/nodes/{nodeId}");
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

        /// <summary>
        /// Returns the version information from Hadoop
        /// </summary>
        /// <returns>Hadoop version</returns>
        public string GetHadoopVersion()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}