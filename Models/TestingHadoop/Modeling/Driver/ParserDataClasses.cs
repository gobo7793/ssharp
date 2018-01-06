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
    /// <summary>
    /// Data for an application from application list.
    /// CMD: <c>yarn application -appStates &lt;states&gt; -list</c>
    /// </summary>
    public class ApplicationListResult
    {
        public ApplicationListResult(string appId, string appName, string appType, 
            AppState state, string finalState, int progess, string trackingUrl)
        {
            AppId = appId;
            AppName = appName;
            AppType = appType;
            State = state;
            FinalState = finalState;
            Progess = progess;
            TrackingUrl = trackingUrl;
        }

        /// <summary>
        /// Application-Id
        /// </summary>
        public string AppId { get; }

        /// <summary>
        /// Application-Name
        /// </summary>
        public string AppName { get; }

        /// <summary>
        /// Application-Type
        /// </summary>
        public string AppType { get; }

        /// <summary>
        /// State
        /// </summary>
        public AppState State { get; }

        /// <summary>
        /// Final-State
        /// </summary>
        public string FinalState { get; }

        /// <summary>
        /// Progress
        /// </summary>
        public int Progess { get; }

        /// <summary>
        /// Tracking-URL
        /// </summary>
        public string TrackingUrl { get; }
    }

    /// <summary>
    /// Data for an application from app details.
    /// CMD: <c>yarn application -status &lt;appId&gt;</c>
    /// </summary>
    public class ApplicationDetailsResult : ApplicationListResult
    {
        public ApplicationDetailsResult(string appId, string appName, string appType, string user, string queue,
                                        AppState state, string finalState, int progess, string trackingUrl, DateTime startTime,
                                        DateTime finishTime, string amHost, int mbSeconds, int vcoreSeconds)
            : base(appId, appName, appType, state, finalState, progess, trackingUrl)
        {
            StartTime = startTime;
            FinishTime = finishTime;
            AmHost = amHost;
            MbSeconds = mbSeconds;
            VcoreSeconds = vcoreSeconds;
        }

        /// <summary>
        /// Start-Time
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// Finish-Time
        /// </summary>
        public DateTime FinishTime { get; }

        /// <summary>
        /// AM Host
        /// </summary>
        public string AmHost { get; }

        /// <summary>
        /// Aggregate Resource Allocation MB-seconds
        /// </summary>
        public int MbSeconds { get; }

        /// <summary>
        /// Aggregate Resource Allocation vcore-seconds
        /// </summary>
        public int VcoreSeconds { get; }
    }

    /// <summary>
    /// Data for an attempts from attempt list.
    /// CMD: <c>yarn applicationattempt -list &lt;appID&gt;</c>
    /// </summary>
    public class ApplicationAttemptListResult
    {
        public ApplicationAttemptListResult(string attemptId, AppState state, string amContainerId, string trackingUrl)
        {
            AttemptId = attemptId;
            State = state;
            AmContainerId = amContainerId;
            TrackingUrl = trackingUrl;
        }

        /// <summary>
        /// ApplicationAttempt-Id
        /// </summary>
        public string AttemptId { get; }

        /// <summary>
        /// State
        /// </summary>
        public AppState State { get; }

        /// <summary>
        /// AM-Container-Id
        /// </summary>
        public string AmContainerId { get; }

        /// <summary>
        /// Tracking-URL
        /// </summary>
        public string TrackingUrl { get; }
    }

    /// <summary>
    /// Data for an application from app details.
    /// CMD: <c>yarn applicationattempt -status &lt;appAttemptId&gt;</c>
    /// </summary>
    public class ApplicationAttemptDetailsResult : ApplicationAttemptListResult
    {
        public ApplicationAttemptDetailsResult(string attemptId, AppState state, string amContainerId, string trackingUrl, string amHost)
            : base(attemptId, state, amContainerId, trackingUrl)
        {
            AmHost = amHost;
        }

        /// <summary>
        /// AM Host
        /// </summary>
        public string AmHost { get; }
    }

    /// <summary>
    /// Data for a running container from the container list for an application attempt.
    /// CMD: <c>yarn container -list &lt;attemptID&gt;</c>
    /// </summary>
    public class ContainerListResult
    {
        public ContainerListResult(string containerId, DateTime startTime, DateTime finishTime,
            AppState state, string host, string logUrl)
        {
            ContainerId = containerId;
            StartTime = startTime;
            FinishTime = finishTime;
            State = state;
            Host = host;
            LogUrl = logUrl;
        }

        /// <summary>
        /// Container-Id
        /// </summary>
        public string ContainerId { get; }

        /// <summary>
        /// Start Time
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// Finish Time
        /// </summary>
        public DateTime FinishTime { get; }

        /// <summary>
        /// State
        /// </summary>
        public AppState State { get; }

        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// LOG-URL
        /// </summary>
        public string LogUrl { get; }

    }

    /// <summary>
    /// Data for a node from the node list.
    /// CMD: <c>yarn node -list</c>
    /// </summary>
    public class NodeListResult
    {
        public NodeListResult(string nodeId, string nodeState, string nodeHttpAdd, int runningContainerCount)
        {
            NodeId = nodeId;
            NodeState = nodeState;
            NodeHttpAdd = nodeHttpAdd;
            RunningContainerCount = runningContainerCount;
        }

        /// <summary>
        /// Node-Id
        /// </summary>
        public string NodeId { get; }

        /// <summary>
        /// Node-State
        /// </summary>
        public string NodeState { get; }

        /// <summary>
        /// Node-Http-Address
        /// </summary>
        public string NodeHttpAdd { get; }

        /// <summary>
        /// Number-of-Running-Containers
        /// </summary>
        public int RunningContainerCount { get; }
    }

    /// <summary>
    /// Data for a node from node details.
    /// CMD: <c>yarn node -status &lt;nodeId&gt;</c>
    /// </summary>
    public class NodeDetailsResult : NodeListResult
    {
        public NodeDetailsResult(string nodeId, string nodeState, string nodeHttpAdd, int runningContainerCount,
            int memoryUsed, int memoryCapacity, int cpuUsed, int cpuCapacity)
            : base(nodeId, nodeState, nodeHttpAdd, runningContainerCount)
        {
            MemoryUsed = memoryUsed;
            MemoryCapacity = memoryCapacity;
            CpuUsed = cpuUsed;
            CpuCapacity = cpuCapacity;
        }

        /// <summary>
        /// Memory-Used in MB
        /// </summary>
        public int MemoryUsed { get; }

        /// <summary>
        /// Memory-Capacity in MB
        /// </summary>
        public int MemoryCapacity { get; }

        /// <summary>
        /// CPU-Used in vcores
        /// </summary>
        public int CpuUsed { get; }

        /// <summary>
        /// CPU-Capacity in vcores
        /// </summary>
        public int CpuCapacity { get; }
    }
}