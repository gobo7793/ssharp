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
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    /*
     * Notes for Parser class names:
     * RM/NM: ResourceManager/NodeManager
     * TL: Timeline server (not available for node and not needed for application)
     * ...Result: The result of one component itself and the full result from TL for getting details
     * ...ListJsonResult: The full result from RM/NM for getting lists
     * ...JsonResultCollection: The list itself from RM/NM or full result from TL for getting lists
     * ...DetailsJsonResult: The full result from RM/NM for getting details (not available for appAttempts)
     */

    /// <summary>
    /// Interface for parsed war YARN components
    /// </summary>
    public interface IParsedComponent { }

    #region Application

    /// <summary>
    /// Data for application attempts
    /// </summary>
    /// <remarks>
    /// CMD List:       <c>yarn applicationattempt -list &lt;appID&gt;</c>
    /// CMD Details:    <c>yarn applicationattempt -status &lt;appAttemptId&gt;</c>
    /// REST List:      <c>http://controller:8088/ws/v1/cluster/apps/{appid}/appattempts</c>
    /// REST TL List:   <c>http://controller:8188/ws/v1/applicationhistory/apps/{appid}/appattempts</c>
    /// REST TL Details:<c>http://controller:8188/ws/v1/applicationhistory/apps/{appid}/appattempts/{appattemptid}</c>
    /// </remarks>
    public interface IApplicationResult : IParsedComponent
    {
        /// <summary>
        /// Application-Id
        /// </summary>
        string AppId { get; set; }

        /// <summary>
        /// Application-Name
        /// </summary>
        string AppName { get; set; }

        /// <summary>
        /// Application-Type
        /// </summary>
        string AppType { get; set; }

        /// <summary>
        /// State
        /// </summary>
        EAppState State { get; set; }

        /// <summary>
        /// Final status
        /// </summary>
        EFinalStatus FinalStatus { get; set; }

        /// <summary>
        /// Progress
        /// </summary>
        float Progess { get; set; }

        /// <summary>
        /// Tracking-URL
        /// </summary>
        string TrackingUrl { get; set; }

        /// <summary>
        /// Start-Time
        /// </summary>
        DateTime StartTime { get; set; }

        /// <summary>
        /// Finish-Time
        /// </summary>
        DateTime FinishTime { get; set; }

        /// <summary>
        /// AM Host
        /// </summary>
        YarnNode AmHost { get; set; }

        /// <summary>
        /// Running containers count
        /// </summary>
        long RunningContainers { get; set; }

        /// <summary>
        /// Allocated Memory in MB
        /// </summary>
        long AllocatedMb { get; set; }

        /// <summary>
        /// Allocated CPU in VCores
        /// </summary>
        long AllocatedVcores { get; set; }

        /// <summary>
        /// Aggregate Resource Allocation MB-seconds
        /// </summary>
        long MbSeconds { get; set; }

        /// <summary>
        /// Aggregate Resource Allocation vcore-seconds
        /// </summary>
        long VcoreSeconds { get; set; }

        /// <summary>
        /// Preempted Memory in MB
        /// </summary>
        long PreemptedMb { get; set; }

        /// <summary>
        /// Preempted CPU in VCores
        /// </summary>
        long PreemptedVcores { get; set; }

        /// <summary>
        /// Preempted Non-AM container count
        /// </summary>
        long NonAmContainerPreempted { get; set; }

        /// <summary>
        /// Preempted AM container count
        /// </summary>
        long AmContainerPreempted { get; set; }

        /// <summary>
        /// Diagnostics info
        /// </summary>
        string Diagnostics { get; set; }

    }

    #endregion

    #region AppAttempt

    /// <summary>
    /// Data for application attempts
    /// </summary>
    /// <remarks>
    /// CMD List:       <c>yarn applicationattempt -list &lt;appID&gt;</c>
    /// CMD Details:    <c>yarn applicationattempt -status &lt;appAttemptId&gt;</c>
    /// REST List:      <c>http://controller:8088/ws/v1/cluster/apps/{appid}/appattempts</c>
    /// REST TL List:   <c>http://controller:8188/ws/v1/applicationhistory/apps/{appid}/appattempts</c>
    /// REST TL Details:<c>http://controller:8188/ws/v1/applicationhistory/apps/{appid}/appattempts/{appattemptid}</c>
    /// </remarks>
    public interface IAppAttemptResult : IParsedComponent
    {
        /// <summary>
        /// ApplicationAttempt-Id
        /// </summary>
        string AttemptId { get; set; }

        /// <summary>
        /// State
        /// </summary>
        EAppState State { get; set; }

        /// <summary>
        /// AM-Container-Id
        /// </summary>
        string AmContainerId { get; set; }

        /// <summary>
        /// Tracking-URL
        /// </summary>
        string TrackingUrl { get; set; }

        /// <summary>
        /// AM Host
        /// </summary>
        YarnNode AmHost { get; set; }

        /// <summary>
        /// Start-Time
        /// </summary>
        DateTime StartTime { get; set; }

        /// <summary>
        /// Diagnostics info
        /// </summary>
        string Diagnostics { get; set; }

        /// <summary>
        /// Logs URL
        /// </summary>
        string LogsUrl { get; set; }

    }

    #endregion

    #region Container

    /// <summary>
    /// Data for running containers
    /// </summary>
    /// <remarks>
    /// CMD List:       <c>yarn container -list &lt;attemptID&gt;</c>
    /// CMD Details:    <c>yarn container -status &lt;containerID&gt;</c>
    /// REST Node List: <c>http://compute-{no}/ws/v1/node/containers</c>
    /// REST Details:   <c>http://compute-{no}/ws/v1/node/containers/{containerid}</c>
    /// 
    /// The Containers for an app can be get via
    /// <c>http://compute-{no.}/ws/v1/node/apps</c> or<c>http://compute-{no.}/ws/v1/node/apps/{appid}</c>
    /// 
    /// The Containers from the timeline server can be get via
    /// <c>http://controller:8188/ws/v1/applicationhistory/apps/{appid}/appattempts/{appattemptid}/containers</c>
    /// <c>http://controller:8188/ws/v1/applicationhistory/apps/{appid}/appattempts/{appattemptid}/containers/{containerid}</c>
    /// </remarks>
    public interface IContainerResult : IParsedComponent
    {
        /// <summary>
        /// Container-Id
        /// </summary>
        string ContainerId { get; set; }

        /// <summary>
        /// Start Time
        /// </summary>
        DateTime StartTime { get; set; }

        /// <summary>
        /// Finish Time
        /// </summary>
        DateTime FinishTime { get; set; }

        /// <summary>
        /// State
        /// </summary>
        EContainerState State { get; set; }

        /// <summary>
        /// Host
        /// </summary>
        YarnNode Host { get; set; }

        /// <summary>
        /// LOG-URL
        /// </summary>
        string LogUrl { get; set; }

        /// <summary>
        /// Priority of container
        /// </summary>
        int Priority { get; set; }

        /// <summary>
        /// Exit code
        /// </summary>
        int ExitCode { get; set; }

        /// <summary>
        /// Diagnostics message for failed containers
        /// </summary>
        string Diagnostics { get; set; }

        /// <summary>
        /// Amount of needed/allocated Memory in MB
        /// </summary>
        long MemoryNeeded { get; set; }

        /// <summary>
        /// Amound of needed/allocated VCores
        /// </summary>
        long VcoresNeeded { get; set; }
    }

    #endregion

    #region Node

    /// <summary>
    /// Data for nodes
    /// CMD: <c>yarn node -list</c>
    /// </summary>
    /// <remarks>
    /// CMD List:       <c>yarn applicationattempt -list &lt;appID&gt;</c>
    /// CMD Details:    <c>yarn applicationattempt -status &lt;appAttemptId&gt;</c>
    /// REST List:      <c>http://controller:8088/ws/v1/cluster/nodes</c>
    /// REST Details:   <c>http://controller:8088/ws/v1/cluster/nodes/{nodeid}</c>
    /// </remarks>
    public interface INodeResult : IParsedComponent
    {
        /// <summary>
        /// Node-Id
        /// </summary>
        string NodeId { get; set; }

        /// <summary>
        /// Node hostname
        /// </summary>
        string Hostname { get; set; }

        /// <summary>
        /// Node-State
        /// </summary>
        ENodeState NodeState { get; set; }

        /// <summary>
        /// Number-of-Running-Containers
        /// </summary>
        int RunningContainerCount { get; set; }

        /// <summary>
        /// Memory-Used in MB
        /// </summary>
        long MemoryUsed { get; set; }

        /// <summary>
        /// Available memory in MB
        /// </summary>
        long MemoryAvailable { get; set; }

        /// <summary>
        /// Memory-Capacity in MB
        /// </summary>
        long MemoryCapacity { get; set; }

        /// <summary>
        /// CPU-Used in vcores
        /// </summary>
        long CpuUsed { get; set; }

        /// <summary>
        /// Available CPU in vcores
        /// </summary>
        long CpuAvailable { get; set; }

        /// <summary>
        /// CPU-Capacity in vcores
        /// </summary>
        long CpuCapacity { get; set; }

        /// <summary>
        /// Health status
        /// </summary>
        string HealthStatus { get; set; }

        /// <summary>
        /// Health Report
        /// </summary>
        string HealthReport { get; set; }

        /// <summary>
        /// Last Health update
        /// </summary>
        DateTime LastHealthUpdate { get; set; }
    }

    #endregion
}