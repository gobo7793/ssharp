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
using System.Diagnostics;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    /// <summary>
    /// Data for an application from application list.
    /// CMD: <c>yarn application -appStates &lt;states&gt; -list</c>
    /// </summary>
    [DebuggerDisplay("Application {" + nameof(AppId) + "}")]
    public class ApplicationListResult
    {
        public ApplicationListResult(string appId, string appName, string appType,
            EAppState state, EFinalStatus finalStatus, int progess, string trackingUrl)
        {
            AppId = appId;
            AppName = appName;
            AppType = appType;
            State = state;
            FinalStatus = finalStatus;
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
        public EAppState State { get; }

        /// <summary>
        /// Final status
        /// </summary>
        public EFinalStatus FinalStatus { get; }

        /// <summary>
        /// Progress
        /// </summary>
        public int Progess { get; }

        /// <summary>
        /// Tracking-URL
        /// </summary>
        public string TrackingUrl { get; }

        public override bool Equals(object obj)
        {
            var ob = obj as ApplicationListResult;
            if(ob == null)
                return false;
            return Equals(ob);
        }

        protected bool Equals(ApplicationListResult other)
        {
            return string.Equals(AppId, other.AppId) && string.Equals(AppName, other.AppName) && string.Equals(AppType, other.AppType) &&
                   State == other.State && FinalStatus == other.FinalStatus && Progess == other.Progess &&
                   string.Equals(TrackingUrl, other.TrackingUrl);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AppId != null ? AppId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AppName != null ? AppName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AppType != null ? AppType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)State;
                hashCode = (hashCode * 397) ^ (int)FinalStatus;
                hashCode = (hashCode * 397) ^ Progess;
                hashCode = (hashCode * 397) ^ (TrackingUrl != null ? TrackingUrl.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    /// <summary>
    /// Data for an application from app details.
    /// CMD: <c>yarn application -status &lt;appId&gt;</c>
    /// </summary>
    [DebuggerDisplay("Application {" + nameof(AppId) + "}")]
    public class ApplicationDetailsResult : ApplicationListResult
    {
        public ApplicationDetailsResult(string appId, string appName, string appType, EAppState state,
            EFinalStatus finalStatus, int progess, string trackingUrl, DateTime startTime,
            DateTime finishTime, YarnNode amHost, int mbSeconds, int vcoreSeconds)
            : base(appId, appName, appType, state, finalStatus, progess, trackingUrl)
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
        public YarnNode AmHost { get; }

        /// <summary>
        /// Aggregate Resource Allocation MB-seconds
        /// </summary>
        public int MbSeconds { get; }

        /// <summary>
        /// Aggregate Resource Allocation vcore-seconds
        /// </summary>
        public int VcoreSeconds { get; }

        public override bool Equals(object obj)
        {
            var ob = obj as ApplicationDetailsResult;
            if(ob == null)
                return false;
            return Equals(ob);
        }

        protected bool Equals(ApplicationDetailsResult other)
        {
            return base.Equals(other) && StartTime.Equals(other.StartTime) && FinishTime.Equals(other.FinishTime) &&
                   Equals(AmHost, other.AmHost) && MbSeconds == other.MbSeconds && VcoreSeconds == other.VcoreSeconds;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ StartTime.GetHashCode();
                hashCode = (hashCode * 397) ^ FinishTime.GetHashCode();
                hashCode = (hashCode * 397) ^ (AmHost != null ? AmHost.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MbSeconds;
                hashCode = (hashCode * 397) ^ VcoreSeconds;
                return hashCode;
            }
        }
    }

    /// <summary>
    /// Data for an attempts from attempt list.
    /// CMD: <c>yarn applicationattempt -list &lt;appID&gt;</c>
    /// </summary>
    [DebuggerDisplay("Attempt {" + nameof(AttemptId) + "}")]
    public class ApplicationAttemptListResult
    {
        public ApplicationAttemptListResult(string attemptId, EAppState state, string amContainerId, string trackingUrl)
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
        public EAppState State { get; }

        /// <summary>
        /// AM-Container-Id
        /// </summary>
        public string AmContainerId { get; }

        /// <summary>
        /// Tracking-URL
        /// </summary>
        public string TrackingUrl { get; }

        public override bool Equals(object obj)
        {
            var ob = obj as ApplicationAttemptListResult;
            if(ob == null)
                return false;
            return Equals(ob);
        }

        protected bool Equals(ApplicationAttemptListResult other)
        {
            return string.Equals(AttemptId, other.AttemptId) && State == other.State && string.Equals(AmContainerId, other.AmContainerId) &&
                   string.Equals(TrackingUrl, other.TrackingUrl);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AttemptId != null ? AttemptId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)State;
                hashCode = (hashCode * 397) ^ (AmContainerId != null ? AmContainerId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TrackingUrl != null ? TrackingUrl.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    /// <summary>
    /// Data for an application from app details.
    /// CMD: <c>yarn applicationattempt -status &lt;appAttemptId&gt;</c>
    /// </summary>
    [DebuggerDisplay("Attempt {" + nameof(AttemptId) + "}")]
    public class ApplicationAttemptDetailsResult : ApplicationAttemptListResult
    {
        public ApplicationAttemptDetailsResult(string attemptId, EAppState state, string amContainerId, string trackingUrl, YarnNode amHost)
            : base(attemptId, state, amContainerId, trackingUrl)
        {
            AmHost = amHost;
        }

        /// <summary>
        /// AM Host
        /// </summary>
        public YarnNode AmHost { get; }

        public override bool Equals(object obj)
        {
            var ob = obj as ApplicationAttemptDetailsResult;
            if(ob == null)
                return false;
            return Equals(ob);
        }

        protected bool Equals(ApplicationAttemptDetailsResult other)
        {
            return base.Equals(other) && Equals(AmHost, other.AmHost);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (AmHost != null ? AmHost.GetHashCode() : 0);
            }
        }
    }

    /// <summary>
    /// Data for a running container from the container list for an application attempt.
    /// Can also be used for container details.
    /// CMD: <c>yarn container -list &lt;attemptID&gt;</c>
    /// CMD: <c>yarn container -status &lt;containerID&gt;</c>
    /// </summary>
    [DebuggerDisplay("Container {" + nameof(ContainerId) + "}")]
    public class ContainerListResult
    {
        public ContainerListResult(string containerId, DateTime startTime, DateTime finishTime,
            EAppState state, YarnNode host, string logUrl)
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
        public EAppState State { get; }

        /// <summary>
        /// Host
        /// </summary>
        public YarnNode Host { get; }

        /// <summary>
        /// LOG-URL
        /// </summary>
        public string LogUrl { get; }

        public override bool Equals(object obj)
        {
            var ob = obj as ContainerListResult;
            if(ob == null)
                return false;
            return Equals(ob);
        }

        protected bool Equals(ContainerListResult other)
        {
            return string.Equals(ContainerId, other.ContainerId) && StartTime.Equals(other.StartTime) &&
                   FinishTime.Equals(other.FinishTime) && State == other.State && Equals(Host, other.Host) &&
                   string.Equals(LogUrl, other.LogUrl);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ContainerId != null ? ContainerId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ StartTime.GetHashCode();
                hashCode = (hashCode * 397) ^ FinishTime.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)State;
                hashCode = (hashCode * 397) ^ (Host != null ? Host.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LogUrl != null ? LogUrl.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    /// <summary>
    /// Data for a node from the node list.
    /// CMD: <c>yarn node -list</c>
    /// </summary>
    [DebuggerDisplay("Node {" + nameof(NodeId) + "}")]
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

        public override bool Equals(object obj)
        {
            var ob = obj as NodeListResult;
            if(ob == null)
                return false;
            return Equals(ob);
        }

        protected bool Equals(NodeListResult other)
        {
            return string.Equals(NodeId, other.NodeId) && string.Equals(NodeState, other.NodeState) &&
                   string.Equals(NodeHttpAdd, other.NodeHttpAdd) && RunningContainerCount == other.RunningContainerCount;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (NodeId != null ? NodeId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NodeState != null ? NodeState.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NodeHttpAdd != null ? NodeHttpAdd.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RunningContainerCount;
                return hashCode;
            }
        }
    }

    /// <summary>
    /// Data for a node from node details.
    /// CMD: <c>yarn node -status &lt;nodeId&gt;</c>
    /// </summary>
    [DebuggerDisplay("Node {" + nameof(NodeId) + "}")]
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

        public override bool Equals(object obj)
        {
            var ob = obj as NodeDetailsResult;
            if(ob == null)
                return false;
            return Equals(ob);
        }

        protected bool Equals(NodeDetailsResult other)
        {
            return base.Equals(other) && MemoryUsed == other.MemoryUsed && MemoryCapacity == other.MemoryCapacity &&
                   CpuUsed == other.CpuUsed && CpuCapacity == other.CpuCapacity;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ MemoryUsed;
                hashCode = (hashCode * 397) ^ MemoryCapacity;
                hashCode = (hashCode * 397) ^ CpuUsed;
                hashCode = (hashCode * 397) ^ CpuCapacity;
                return hashCode;
            }
        }
    }
}