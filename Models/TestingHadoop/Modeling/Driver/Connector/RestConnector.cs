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
using System.Linq;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Connector
{
    /// <summary>
    /// Hadoop connector for monitoring using the REST API
    /// </summary>
    public class RestConnector : IHadoopConnector
    {

        #region Properties

        private static RestConnector _Instance;

        /// <summary>
        /// <see cref="RestConnector"/> instance
        /// </summary>
        public static RestConnector Instance => _Instance ?? CreateInstance();

        /// <summary>
        /// The monitoring connections
        /// </summary>
        private Dictionary<int, SshConnection> MonitoringConnections { get; } = new Dictionary<int, SshConnection>();

        /// <summary>
        /// Monitoring connection for RM
        /// </summary>
        private SshConnection MonitoringRm => MonitoringConnections.FirstOrDefault().Value;

        /// <summary>
        /// ResourceManager URL
        /// </summary>
        private string RmUrl => ModelSettings.ControllerRestRmUrl;

        /// <summary>
        /// Timeline server URL
        /// </summary>
        private string TlUrl => ModelSettings.ControllerRestTlsUrl;

        /// <summary>
        /// The curl base command
        /// </summary>
        private const string Curl = "curl -H \"Accept: application/json\" -X GET";

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new connection to the hadoop cluster only for monitoring using the REST API.
        /// Note that one connection is needed for Monitoring, so maybe set <c>/etc/ssh/sshd_config</c>
        /// MaxSessions to the needed value (default 10)!
        /// </summary>
        private RestConnector()
        {
            for(int i = 0; i < ModelSettings.HostsCount; i++)
                MonitoringConnections[i + 1] = (new SshConnection(ModelSettings.SshHosts[i], ModelSettings.SshUsernames[i],
                    ModelSettings.SshPrivateKeyFiles[i], $"resMonH{i + 1}"));
        }

        /// <summary>
        /// Creates a new <see cref="RestConnector"/> instance, saves and returns it if <see cref="ModelSettings.SshHosts"/> are available
        /// </summary>
        /// <returns>Null if <see cref="ModelSettings.SshHosts"/> is not set, otherwise the instance</returns>
        private static RestConnector CreateInstance()
        {
            if(ModelSettings.SshHosts.Length < 1)
                return null;
            _Instance = new RestConnector();
            return _Instance;
        }

        /// <summary>
        /// Executes the given monitoring command for monitoring a compute node
        /// </summary>
        /// <param name="cmd">The monitoring command</param>
        /// <param name="nodeId">ID number of the node</param>
        /// <returns>The result or on connection refused <see cref="String.Empty"/></returns>
        private string MonitorCompute(string cmd, int nodeId)
        {
            string result;
            if(nodeId > 8041 && ModelSettings.HostMode == ModelSettings.EHostMode.Multihost)
            {
                var hostId = DriverUtilities.GetHostId(nodeId - 8041, ModelSettings.HostsCount, ModelSettings.NodeBaseCount);
                result = MonitoringConnections[hostId].Run(cmd);
            }
            else
            {
                result = MonitoringRm.Run(cmd);
            }

            result = result.Trim();
            if(result.EndsWith("Connection refused") ||
               result.EndsWith("Connection reset by peer") ||
               result.EndsWith("</html>"))
                return String.Empty;
            return result;
        }

        /// <summary>
        /// Resets the <see cref="RestConnector"/> instance
        /// </summary>
        public static void ResetInstance()
        {
            if(_Instance == null)
                return;

            //_Instance?.Dispose();
            foreach(var con in _Instance.MonitoringConnections)
                con.Value.Disconnect();
            _Instance.MonitoringConnections.Clear();

            _Instance = null;
        }

        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            foreach(var conn in MonitoringConnections)
                conn.Value.Dispose();
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
            var cmd = $"{Curl} {RmUrl}/ws/v1/cluster/apps";
            if(!String.IsNullOrWhiteSpace(states))
            {
                states = states.Replace("ALL", "").Replace("None", "").Replace("NotStartedYet", "");
                cmd = $"{cmd}?states={states}";
            }

            return MonitoringRm.Run(cmd);
        }

        /// <summary>
        /// Gets the YARN application attempt list itself for the given app id
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        public string GetYarnAppAttemptList(string appId)
        {
            return MonitoringRm.Run($"{Curl} {RmUrl}/ws/v1/cluster/apps/{appId}/appattempts");
        }

        /// <summary>
        /// Gets the YARN application attempt list itself for the given app id from the timeline server
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        public string GetYarnAppAttemptListTl(string appId)
        {
            return MonitoringRm.Run($"{Curl} {TlUrl}/ws/v1/applicationhistory/apps/{appId}/appattempts");
        }

        /// <summary>
        /// Gets the YARN application container list itself for the given node http url
        /// </summary>
        /// <param name="nodeUrl">The attempt or node id</param>
        /// <returns>The YARN application container list</returns>
        public string GetYarnAppContainerList(string nodeUrl)
        {
            var nodeId = DriverUtilities.ParseInt(nodeUrl);
            return MonitorCompute($"{Curl} {nodeUrl}/ws/v1/node/containers", nodeId);
        }

        /// <summary>
        /// Gets the YARN application container list itself for the given application attempt id from the timeline server
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application container list</returns>
        public string GetYarnAppContainerListTl(string attemptId)
        {
            var appId = DriverUtilities.ConvertId(attemptId, EConvertType.App);

            return MonitoringRm.Run($"{Curl} {TlUrl}/ws/v1/applicationhistory/apps/{appId}/appattempts/{attemptId}/containers");
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
            return MonitoringRm.Run($"{Curl} {RmUrl}/ws/v1/cluster/apps/{appId}");
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
            var appId = DriverUtilities.ConvertId(attemptId, EConvertType.App);

            return MonitoringRm.Run($"{Curl} {TlUrl}/ws/v1/applicationhistory/apps/{appId}/appattempts/{attemptId}");
        }

        /// <summary>
        /// Gets the YARN application container details itself for the given container id from the given node
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <param name="nodeUrl">Node URL of the container</param>
        /// <returns>The YARN application container details</returns>
        public string GetYarnAppContainerDetails(string containerId, string nodeUrl = null)
        {
            if(nodeUrl == null)
                throw new ArgumentNullException($"No node url given to get details for container {containerId}!");

            var nodeId = DriverUtilities.ParseInt(nodeUrl);
            return MonitorCompute($"{Curl} {nodeUrl}/ws/v1/node/containers/{containerId}", nodeId);
        }

        /// <summary>
        /// Gets the YARN application container details itself for the given container id from the timeline server
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <returns>The YARN application container details</returns>
        public string GetYarnAppContainerDetailsTl(string containerId)
        {
            var attemptId = DriverUtilities.ConvertId(containerId, EConvertType.Attempt);
            var appId = DriverUtilities.ConvertId(containerId, EConvertType.App);

            return MonitoringRm.Run($"{Curl} {TlUrl}/ws/v1/applicationhistory/apps/{appId}/appattempts/{attemptId}/containers/{containerId}");
        }

        #endregion

        #region YARN Nodes

        /// <summary>
        /// Gets the YARN node list itself
        /// </summary>
        /// <returns>The YARN node list</returns>
        public string GetYarnNodeList()
        {
            return MonitoringRm.Run($"{Curl} {RmUrl}/ws/v1/cluster/nodes");
        }

        /// <summary>
        /// Gets the YARN node details itself for the given node id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>The YARN node details</returns>
        public string GetYarnNodeDetails(string nodeId)
        {
            return MonitoringRm.Run($"{Curl} {RmUrl}/ws/v1/cluster/nodes/{nodeId}");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public bool StartNode(string nodeName)
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support starting node!");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public bool StopNode(string nodeName)
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support stopping node!");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public bool StartNodeNetConnection(string nodeName)
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support starting node connection!");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public bool StopNodeNetConnection(string nodeName)
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support stopping node connection!");
        }

        #endregion

        #region Application Control

        /// <summary>
        /// Not supported
        /// </summary>
        public bool KillApplication(string appId)
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support killing application!");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public string StartApplication(string cmd)
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support starting application!");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public string StartApplicationAsyncTillId(string cmd)
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support starting application async!");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public void StartApplicationAsyncFull(string cmd)
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support starting application async!");
        }

        #endregion

        #region HDFS related

        /// <summary>
        /// Not supported
        /// </summary>
        public void RunHdfsFsckDelete()
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support hdfs file check!");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public bool ExistsHdfsDir(string directory)
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support check if hdfs directories exists!");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public void RemoveHdfsDir(string directory)
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support remove hdfs directories!");
        }

        #endregion

        #region Other

        /// <summary>
        /// Not supported
        /// </summary>
        public bool StartCluster(string config = "")
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support starting cluster!");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public bool StopCluster()
        {
            throw new PlatformNotSupportedException("Hadoop REST API does not support stopping cluster!");
        }

        /// <summary>
        /// Returns the version information from Hadoop
        /// </summary>
        /// <returns>Hadoop version</returns>
        public string GetHadoopVersion()
        {
            return MonitoringRm.Run($"{Curl} {RmUrl}/ws/v1/cluster/info");
        }

        #endregion
    }

}