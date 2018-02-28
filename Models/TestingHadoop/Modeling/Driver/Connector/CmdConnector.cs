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

        /// <summary>
        /// The monitoring connection
        /// </summary>
        private SshConnection Monitoring { get; }

        /// <summary>
        /// The fault handling connection
        /// </summary>
        private SshConnection Faulting { get; }

        /// <summary>
        /// The application submitting connections
        /// </summary>
        private List<SshConnection> Submitting { get; }

        /// <summary>
        /// The controller/timeline host to connect
        /// </summary>
        private string Host { get; }

        /// <summary>
        /// True on console out the commands and results
        /// </summary>
        private bool IsConsoleOut { get; }

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
        /// <param name="forMonitoring">True to use this connection for monitoring</param>
        /// <param name="forFaulting">True to use this connection for fault handling</param>
        /// <param name="submittingConnections">The count for submitting application connections</param>
        /// <param name="isConsoleOut">True on console outputting the commands and returns</param>
        public CmdConnector(string host, string username, string privateKeyFilePath, bool forMonitoring = true,
                            bool forFaulting = true, int submittingConnections = 4, bool isConsoleOut = false)
        {
            Submitting = new List<SshConnection>();

            Host = host;
            IsConsoleOut = isConsoleOut;

            if(forMonitoring)
                Monitoring = new SshConnection(Host, username, privateKeyFilePath);
            if(forFaulting)
                Faulting = new SshConnection(Host, username, privateKeyFilePath);
            for(int i = 0; i < submittingConnections; i++)
                Submitting.Add(new SshConnection(Host, username, privateKeyFilePath));

            //ThreadPool.SetMaxThreads(submittingConnections, submittingConnections);
        }

        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {
            Monitoring?.Dispose();
            Faulting?.Dispose();
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
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            var cmd = $"{Model.HadoopSetupScript} cmd yarn application -list";
            if(!String.IsNullOrWhiteSpace(states))
                cmd = $"{cmd} -appStates {states}";

            return Monitoring.Run(cmd, IsConsoleOut);
        }

        /// <summary>
        /// Gets the YARN application attempt list itself for the given app id
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The YARN application attempt list</returns>
        public string GetYarnAppAttemptList(string appId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{Model.HadoopSetupScript} cmd yarn applicationattempt -list {appId}", IsConsoleOut);
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
        /// Gets the YARN application container list itself for the given application attempt id
        /// </summary>
        /// <param name="id">The attempt id</param>
        /// <returns>The YARN application container list</returns>
        public string GetYarnAppContainerList(string id)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{Model.HadoopSetupScript} cmd yarn container -list {id}", IsConsoleOut);
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
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{Model.HadoopSetupScript} cmd yarn application -status {appId}", IsConsoleOut);
        }

        /// <summary>
        /// Gets the YARN application attempt details itself for the given attempt id
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application attempt details</returns>
        public string GetYarnAppAttemptDetails(string attemptId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{Model.HadoopSetupScript} cmd yarn applicationattempt -status {attemptId}", IsConsoleOut);
        }

        /// <summary>
        /// Gets the YARN application attempt details itself for the given attempt id from the timeline server
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The YARN application attempt details</returns>
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
        public string GetYarnAppContainerDetails(string containerId, string nodeUrl = null)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{Model.HadoopSetupScript} cmd yarn container -status {containerId}", IsConsoleOut);
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
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{Model.HadoopSetupScript} cmd yarn node -list -all", IsConsoleOut);
        }

        /// <summary>
        /// Gets the YARN node details itself for the given node id
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>The YARN node details</returns>
        public string GetYarnNodeDetails(string nodeId)
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{Model.HadoopSetupScript} cmd yarn node -status {nodeId}", IsConsoleOut);
        }

        /// <summary>
        /// Starts the docker container for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if YARN Node started successfully</returns>
        public bool StartNode(string nodeName)
        {
            if(Faulting == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for faulting initialized!");

            var id = DriverUtilities.ParseInt(nodeName);
            Faulting.Run($"{Model.HadoopSetupScript} hadoop start {id}", IsConsoleOut);
            var runCheckRes = Faulting.Run($"{Model.HadoopSetupScript} hadoop info {id} | grep Running", IsConsoleOut);
            return runCheckRes.Contains("true");
        }

        /// <summary>
        /// Stops the docker container for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if YARN Node stopped successfully</returns>
        public bool StopNode(string nodeName)
        {
            if(Faulting == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for faulting initialized!");

            var id = DriverUtilities.ParseInt(nodeName);
            Faulting.Run($"{Model.HadoopSetupScript} hadoop stop {id}", IsConsoleOut);
            var runCheckRes = Faulting.Run($"{Model.HadoopSetupScript} hadoop info {id} | grep Running", IsConsoleOut);
            return runCheckRes.Contains("false");
        }

        /// <summary>
        /// Starts the docker container network connection for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if network connection started successfully</returns>
        public bool StartNodeNetConnection(string nodeName)
        {
            if(Faulting == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for faulting initialized!");

            var id = DriverUtilities.ParseInt(nodeName);
            Faulting.Run($"{Model.HadoopSetupScript} net start {id}", IsConsoleOut);
            var runCheckRes = Faulting.Run($"{Model.HadoopSetupScript} hadoop info {id} | grep NetworkID", IsConsoleOut);
            return !String.IsNullOrWhiteSpace(runCheckRes);
        }

        /// <summary>
        /// Stops the docker container network connection for the given node and returns true if no errors occurs
        /// </summary>
        /// <param name="nodeName">Node name</param>
        /// <returns>True if network connection stopped successfully</returns>
        public bool StopNodeNetConnection(string nodeName)
        {
            if(Faulting == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for faulting initialized!");

            var id = DriverUtilities.ParseInt(nodeName);
            Faulting.Run($"{Model.HadoopSetupScript} net stop {id}", IsConsoleOut);
            var runCheckRes = Faulting.Run($"{Model.HadoopSetupScript} hadoop info {id} | grep NetworkID", IsConsoleOut);
            return String.IsNullOrWhiteSpace(runCheckRes);
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
            if(Faulting == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for faulting initialized!");

            var cmd = Faulting.Run($"{Model.HadoopSetupScript} cmd yarn application -kill {appId} | grep {appId}", IsConsoleOut);
            return cmd.Contains($"Killed application {appId}") || cmd.Contains($"Killing application {appId}") ||
                   cmd.Contains($"{appId} has already finished");
        }

        /// <summary>
        /// Submits the given application with the given arguments to Hadoop
        /// </summary>
        /// <param name="cmd">The command to submit</param>
        public void StartApplication(string cmd)
        {
            var submitter = GetSubmitter(cmd);

            submitter.Run($"{Model.BenchmarkStartupScript} {cmd}", IsConsoleOut);
            //submitter.Run($"{Model.BenchmarkStartupScript} {cmd}", true);
        }

        /// <summary>
        /// Submits the given application with the given arguments to Hadoop async
        /// </summary>
        /// <param name="cmd">The command to submit</param>
        public void StartApplicationAsync(string cmd)
        {
            var submitter = GetSubmitter(cmd);

            submitter.RunAsync($"{Model.BenchmarkStartupScript} {cmd}", IsConsoleOut);
            //submitter.Run($"{Model.BenchmarkStartupScript} {cmd}", true);
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
                    if(sleepCnt > 100)
                        throw new TimeoutException($"No free application submitter available for starting application {cmd}.");
                    Thread.Sleep(100); // waiting for free submitter
                }
            } while(submitter == null);
            return submitter;
        }

        /// <summary>
        /// Returns the version information from Hadoop
        /// </summary>
        /// <returns>Hadoop version</returns>
        public string GetHadoopVersion()
        {
            if(Monitoring == null)
                throw new InvalidOperationException($"{nameof(CmdConnector)} not for monitoring initialized!");

            return Monitoring.Run($"{Model.HadoopSetupScript} cmd yarn version", IsConsoleOut);
        }

        #endregion
    }
}