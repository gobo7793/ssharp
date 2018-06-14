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
using System.Threading;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Connector
{
    /// <summary>
    /// Hadoop connector for using the cmd line
    /// </summary>
    public class CmdConnector : IHadoopConnector
    {

        #region Properties

        private static CmdConnector _Instance;

        /// <summary>
        /// <see cref="CmdConnector"/> instance
        /// </summary>
        public static CmdConnector Instance => _Instance ?? CreateFullInstance();

        /// <summary>
        /// The monitoring connection
        /// </summary>
        private SshConnection Monitoring { get; set; }

        /// <summary>
        /// The fault handling connection (one-based index)
        /// </summary>
        private Dictionary<int, SshConnection> Faulting { get; } = new Dictionary<int, SshConnection>();

        /// <summary>
        /// The application submitting connections
        /// </summary>
        private List<SshConnection> Submitting { get; } = new List<SshConnection>();

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new connection to the hadoop cluster.
        /// Note that one connection is needed for Monitoring, so maybe set <c>/etc/ssh/sshd_config</c>
        /// MaxSessions to the needed value (default 10)!
        /// </summary>
        /// <param name="forMonitoring">True to use this connection for monitoring</param>
        /// <param name="forFaulting">True to use this connection for fault handling</param>
        /// <param name="submittingConnections">The count for submitting application connections</param>
        private CmdConnector(bool forMonitoring = false, bool forFaulting = true, int submittingConnections = 4)
        {
            if(forMonitoring)
                Monitoring = new SshConnection(ModelSettings.SshHosts[0], ModelSettings.SshUsernames[0], ModelSettings.SshPrivateKeyFiles[0],
                    "cmdMonH1");
            if(forFaulting)
                for(int i = 0; i < ModelSettings.HostsCount; i++)
                    Faulting[i + 1] = new SshConnection(ModelSettings.SshHosts[i], ModelSettings.SshUsernames[i], ModelSettings.SshPrivateKeyFiles[i],
                    $"cmdFauH{i + 1}");
            for(int i = 0; i < submittingConnections; i++)
                Submitting.Add(new SshConnection(ModelSettings.SshHosts[0], ModelSettings.SshUsernames[0], ModelSettings.SshPrivateKeyFiles[0],
                    @"Submitted application (application_\d+_\d+)", $"cmdSu{i}H1"));
        }

        /// <summary>
        /// Creates a new <see cref="CmdConnector"/> instance without monitoring features,
        /// saves and returns it if <see cref="ModelSettings.SshHosts"/> is set
        /// </summary>
        /// <returns>Null if <see cref="ModelSettings.SshHosts"/> is not set, otherwise the instance</returns>
        internal static CmdConnector CreateInstance()
        {
            if(String.IsNullOrWhiteSpace(ModelSettings.SshHosts[0]))
                return null;
            _Instance = new CmdConnector();
            return _Instance;
        }

        /// <summary>
        /// Creates a new <see cref="CmdConnector"/> instance with monitoring features,
        /// saves and returns it if <see cref="ModelSettings.SshHosts"/> is set
        /// </summary>
        /// <returns>Null if <see cref="ModelSettings.SshHosts"/> is not set, otherwise the instance</returns>
        internal static CmdConnector CreateFullInstance()
        {
            if(String.IsNullOrWhiteSpace(ModelSettings.SshHosts[0]))
                return null;
            _Instance = new CmdConnector(true);
            return _Instance;
        }

        /// <summary>
        /// Resets the <see cref="CmdConnector"/> instance
        /// </summary>
        public static void ResetInstance()
        {
            if(Instance == null)
                return;

            //_Instance?.Dispose();
            _Instance.Monitoring?.Disconnect();
            foreach(var con in _Instance.Faulting)
                con.Value.Disconnect();
            foreach(var con in _Instance.Submitting)
                con.Disconnect();
            _Instance.Monitoring = null;
            _Instance.Faulting.Clear();
            _Instance.Submitting.Clear();
            _Instance = null;
        }

        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            Monitoring?.Dispose();
            foreach(var con in Faulting)
                con.Value.Dispose();
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
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetYarnApplicationList(string states)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            var cmd = $"{ModelSettings.HadoopSetupScript} cmd yarn application -list";
            if(!String.IsNullOrWhiteSpace(states))
                cmd = $"{cmd} -appStates {states}";

            return Monitoring.Run(cmd);
        }

        /// <summary>
        /// Gets the YARN application attempt list itself for the given app id
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetYarnAppAttemptList(string appId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{ModelSettings.HadoopSetupScript} cmd yarn applicationattempt -list {appId}");
        }

        /// <summary>
        /// Gets the YARN application attempt list itself for the given app id from the timeline server
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetYarnAppAttemptListTl(string appId)
        {
            return GetYarnAppAttemptList(appId);
        }

        /// <summary>
        /// Gets the YARN application container list itself for the given application attempt id
        /// </summary>
        /// <param name="id">The attempt id</param>
        /// <returns>The YARN application container list</returns>
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetYarnAppContainerList(string id)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{ModelSettings.HadoopSetupScript} cmd yarn container -list {id}");
        }

        /// <summary>
        /// Gets the YARN application container list itself for the given application attempt id from the timeline server
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application container list</returns>
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
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
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetYarnApplicationDetails(string appId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{ModelSettings.HadoopSetupScript} cmd yarn application -status {appId}");
        }

        /// <summary>
        /// Gets the YARN application attempt details itself for the given attempt id
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application attempt details</returns>
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetYarnAppAttemptDetails(string attemptId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{ModelSettings.HadoopSetupScript} cmd yarn applicationattempt -status {attemptId}");
        }

        /// <summary>
        /// Gets the YARN application attempt details itself for the given attempt id from the timeline server
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application attempt details</returns>
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetYarnAppAttemptDetailsTl(string attemptId)
        {
            return GetYarnAppAttemptDetails(attemptId);
        }

        /// <summary>
        /// Gets the YARN application container details itself for the given container id
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <param name="nodeUrl">Not needed</param>
        /// <returns>The YARN application container details</returns>
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetYarnAppContainerDetails(string containerId, string nodeUrl = null)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{ModelSettings.HadoopSetupScript} cmd yarn container -status {containerId}");
        }

        /// <summary>
        /// Gets the YARN application container details itself for the given container id from the timeline server
        /// </summary>
        /// <param name="containerId">The container id</param>
        /// <returns>The YARN application container details</returns>
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
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
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetYarnNodeList()
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{ModelSettings.HadoopSetupScript} cmd yarn node -list -all");
        }

        /// <summary>
        /// Gets the YARN node details itself for the given node id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>The YARN node details</returns>
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetYarnNodeDetails(string nodeId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{ModelSettings.HadoopSetupScript} cmd yarn node -status {nodeId}");
        }

        /// <summary>
        /// Starts the docker container for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if YARN Node started successfully</returns>
        /// <exception cref="InvalidOperationException">Faulting not initialized</exception>
        public bool StartNode(string nodeName)
        {
            if(Faulting.Count < 1)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for faulting initialized!");

            var id = DriverUtilities.ParseInt(nodeName);
            var hostId = DriverUtilities.GetHostId(id, ModelSettings.HostsCount, ModelSettings.NodeBaseCount);
            var controlerIp = GetControllerIp();

            Faulting[hostId].Run($"{ModelSettings.HadoopSetupScript} hadoop start {id} {controlerIp}");
            return CheckNodeRunning(id, hostId);
        }

        /// <summary>
        /// Stops the docker container for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if YARN Node stopped successfully</returns>
        /// <exception cref="InvalidOperationException">Faulting not initialized</exception>
        public bool StopNode(string nodeName)
        {
            if(Faulting.Count < 1)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for faulting initialized!");

            var id = DriverUtilities.ParseInt(nodeName);
            var hostId = DriverUtilities.GetHostId(id, ModelSettings.HostsCount, ModelSettings.NodeBaseCount);

            Faulting[hostId].Run($"{ModelSettings.HadoopSetupScript} hadoop stop {id}");
            return !CheckNodeRunning(id, hostId);
        }

        /// <summary>
        /// Check node running and returns true if node docker container is running
        /// </summary>
        /// <param name="nodeId">Node id</param>
        /// <param name="hostId">The host id of the node</param>
        /// <returns>True on node running</returns>
        private bool CheckNodeRunning(int nodeId, int hostId)
        {
            var runCheckRes = Faulting[hostId].Run($"{ModelSettings.HadoopSetupScript} hadoop info {nodeId} '{{{{.State.Running}}}}'");
            return runCheckRes.Contains("true");
        }

        /// <summary>
        /// Starts the docker container network connection for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if network connection started successfully</returns>
        /// <exception cref="InvalidOperationException">Faulting not initialized</exception>
        public bool StartNodeNetConnection(string nodeName)
        {
            if(Faulting.Count < 1)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for faulting initialized!");

            //var id = DriverUtilities.ParseInt(nodeName);
            //var hostId = DriverUtilities.GetHostId(id, Model.HostsCount, Model.NodeBaseCount);

            //Faulting[hostId].Run($"{Model.HadoopSetupScript} net start {id}");
            //return CheckNodeNetwork(id, hostId);

            // Workaround to reconnect compute to controller
            StopNode(nodeName);
            return StartNode(nodeName);
        }

        /// <summary>
        /// Stops the docker container network connection for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if network connection stopped successfully</returns>
        /// <exception cref="InvalidOperationException">Faulting not initialized</exception>
        public bool StopNodeNetConnection(string nodeName)
        {
            if(Faulting.Count < 1)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for faulting initialized!");

            var id = DriverUtilities.ParseInt(nodeName);
            var hostId = DriverUtilities.GetHostId(id, ModelSettings.HostsCount, ModelSettings.NodeBaseCount);

            Faulting[hostId].Run($"{ModelSettings.HadoopSetupScript} net stop {id}");
            return !CheckNodeNetwork(id, hostId);
        }

        /// <summary>
        /// Check network connectivity and returns true if network is set for node docker container
        /// </summary>
        /// <param name="nodeId">Node id</param>
        /// <param name="hostId">The host id of the node</param>
        /// <returns>True on network connectivity</returns>
        private bool CheckNodeNetwork(int nodeId, int hostId)
        {
            var runCheckRes = Faulting[hostId]
                .Run($"{ModelSettings.HadoopSetupScript} hadoop info {nodeId} '{{{{.NetworkSettings.Networks}}}}'");
            return runCheckRes.Contains("hadoop-net");
        }

        /// <summary>
        /// Gets the IP of the controller in multihost mode
        /// </summary>
        /// <returns>The ip</returns>
        /// <exception cref="InvalidOperationException">If cluster is not in multihost mode</exception>
        private string GetControllerIp()
        {
            if(ModelSettings.HostMode != ModelSettings.EHostMode.Multihost)
                throw new InvalidOperationException("Starting cluster needs multihost cluster mode!");
            return Faulting[1].Run($"{ModelSettings.HadoopSetupScript} controllerip").Trim();
        }

        #endregion

        #region Application Control

        /// <summary>
        /// Kills the given application and returns true if no errors occurs
        /// </summary>
        /// <param name="appId">The app id for the application</param>
        /// <returns>True if application killed</returns>
        /// <exception cref="InvalidOperationException">Faulting not initialized</exception>
        public bool KillApplication(string appId)
        {
            if(Faulting.Count < 1)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for faulting initialized!");

            var cmd = Faulting[1].Run($"{ModelSettings.HadoopSetupScript} cmd yarn application -kill {appId} | grep {appId}");
            return cmd.Contains($"Killed application {appId}") || cmd.Contains($"Killing application {appId}") ||
                   cmd.Contains($"{appId} has already finished") || cmd.Contains($"'{appId}' doesn't exist");
        }

        /// <summary>
        /// Submits the given application with the given arguments to Hadoop,
        /// waits for end of execution and returns it application id.
        /// If no application id found all output will be returned.
        /// </summary>
        /// <param name="cmd">The command to submit</param>
        /// <returns>The application id for the submitted app</returns>
        /// <exception cref="InvalidOperationException">Application submitting not initialized</exception>
        public string StartApplication(string cmd)
        {
            var submitter = GetSubmitter(cmd);

            return submitter.Run($"{ModelSettings.BenchmarkStartupScript} {cmd}");
            //submitter.Run($"{Model.BenchmarkStartupScript} {cmd}", true);
        }

        /// <summary>
        /// Submits the given application with the given arguments to Hadoop,
        /// waits for the application id and returns it immediately.
        /// If no application id found all output will be returned.
        /// </summary>
        /// <param name="cmd">The command to submit</param>
        /// <returns>The application id for the submitted app</returns>
        /// <exception cref="InvalidOperationException">Application submitting not initialized</exception>
        public string StartApplicationAsyncTillId(string cmd)
        {
            var submitter = GetSubmitter(cmd);

            return submitter.RunAttachedTillAppId($"{ModelSettings.BenchmarkStartupScript} {cmd}");
            //submitter.Run($"{Model.BenchmarkStartupScript} {cmd}", true);
        }

        /// <summary>
        /// Submits the given application with the given arguments to Hadoop.
        /// The application will be fully executed async with no return values.
        /// </summary>
        /// <param name="cmd">The command to submit</param>
        public void StartApplicationAsyncFull(string cmd)
        {
            var submitter = GetSubmitter(cmd);

            submitter.RunAsync($"{ModelSettings.BenchmarkStartupScript} {cmd}");
        }

        private SshConnection GetSubmitter(string cmd)
        {
            if(Submitting.Count < 1)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for starting applications initialized!");

            var sleepCnt = 0;
            SshConnection submitter;
            do
            {
                submitter = Submitting.FirstOrDefault(c => !c.InUse);
                if(submitter == null)
                {
                    ++sleepCnt;
                    if(sleepCnt > 30)
                        throw new TimeoutException($"No free application submitter available for starting application {cmd}.");
                    Thread.Sleep(1000); // waiting for free submitter
                }
            } while(submitter == null);
            return submitter;
        }

        #endregion

        #region HDFS related

        /// <summary>
        /// Runs the hdfs file system check and delete corrupted/missing files.
        /// </summary>
        public void RunHdfsFsckDelete()
        {
            var cmd = $"{ModelSettings.HadoopSetupScript} hdfs fsck / -delete";
            var submitter = GetSubmitter(cmd);

            submitter.Run(cmd);
        }

        /// <summary>
        /// Checks if the given directory exists on hdfs
        /// </summary>
        /// <param name="directory">The directory to check</param>
        /// <returns>True if the directory exists</returns>
        public bool ExistsHdfsDir(string directory)
        {
            var cmd = $"{ModelSettings.HadoopSetupScript} hdfs dfs -test -e {directory}";
            var submitter = GetSubmitter(cmd);

            var exitCode = submitter.Run(cmd);
            return exitCode.Trim() == "0";
        }

        /// <summary>
        /// Removes the given directory on hdfs
        /// </summary>
        /// <param name="directory">The directory to remove</param>
        /// <exception cref="InvalidOperationException">Application submitting not initialized</exception>
        public void RemoveHdfsDir(string directory)
        {
            var cmd = $"{ModelSettings.HadoopSetupScript} cmd hdfs dfs -rm -r {directory}";
            var submitter = GetSubmitter(cmd);

            submitter.Run(cmd);
        }

        #endregion

        #region Other

        /// <summary>
        /// Starts the whole hadoop cluster
        /// </summary>
        /// <param name="config">The config to use to start</param>
        /// <returns>True if the last node on host1 is running</returns>
        /// <exception cref="InvalidOperationException">
        /// Cluster mode is not multihost mode or faulting not initialized
        /// </exception>
        public bool StartCluster(string config = "")
        {
            return ExecuteStartStopCluster("start", config);
        }

        /// <summary>
        /// Stops the whole hadoop cluster
        /// </summary>
        /// <returns>True if the cluster is not running</returns>
        /// <exception cref="InvalidOperationException">
        /// Cluster mode is not multihost mode or faulting not initialized
        /// </exception>
        public bool StopCluster()
        {
            return !ExecuteStartStopCluster("stop");
        }

        /// <summary>
        /// Executes start/stop cluster
        /// </summary>
        /// <param name="action">The action (start/stop)</param>
        /// <param name="config">The cluster config</param>
        /// <returns>True if the cluster is started</returns>
        /// <remarks>
        /// To check if the cluster is started, the last node on host1 will be checked for running.
        /// </remarks>
        private bool ExecuteStartStopCluster(string action, string config = "")
        {
            if(Faulting.Count < 1)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for faulting initialized!");

            var cfgArg = !String.IsNullOrWhiteSpace(config) ? $"-c {config} " : String.Empty;

            if(ModelSettings.HostMode == ModelSettings.EHostMode.DockerMachine)
            {
                var cmd = $"{ModelSettings.HadoopSetupScript} {cfgArg}cluster {action}";
                Faulting[1].Run(cmd);
                var isNodeRunning = CheckNodeRunning(ModelSettings.NodeBaseCount, 1);
                return isNodeRunning;
            }
            if(ModelSettings.HostMode == ModelSettings.EHostMode.Multihost)
            {
                var cmdBase = $"{ModelSettings.HadoopSetupScript} {cfgArg}{action} host";

                // host 1
                Faulting[1].Run($"{cmdBase} 1");
                var ip = GetControllerIp();

                // other hosts
                if(action.StartsWith("start"))
                    Thread.Sleep(5000); // waiting controller is started
                for(int i = 2; i <= ModelSettings.HostsCount; i++)
                {
                    Faulting[i].Run($"{cmdBase} {i} {ip}");
                }

                var isNodeRunning = CheckNodeRunning(ModelSettings.NodeBaseCount, 1);
                return isNodeRunning;
            }
            return false;
        }

        /// <summary>
        /// Gets the current MARP value from the controllers scheduler
        /// </summary>
        /// <returns>The current MARP value</returns>
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetMarpValue()
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            var cmd = $"{ModelSettings.HadoopSetupScript} marp";
            return Monitoring.Run(cmd).Trim();
        }

        /// <summary>
        /// Returns the version information from Hadoop
        /// </summary>
        /// <returns>Hadoop version</returns>
        /// <exception cref="InvalidOperationException">Monitoring not initialized</exception>
        public string GetHadoopVersion()
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{ModelSettings.HadoopSetupScript} cmd yarn version");
        }

        #endregion
    }
}