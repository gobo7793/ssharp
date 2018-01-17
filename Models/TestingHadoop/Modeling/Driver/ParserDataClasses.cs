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
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    /// <summary>
    /// Data for applications
    /// </summary>
    /// <remarks>
    /// CMD List:       <c>yarn application -appStates &lt;states&gt; -list</c>
    /// CMD Details:    <c>yarn application -status &lt;appId&gt;</c>
    /// REST List:      <c>http://controller:8088/ws/v1/cluster/apps</c>
    /// REST Details:   <c>http://controller:8088/ws/v1/cluster/apps/{appid}</c>
    /// </remarks>
    [DebuggerDisplay("Application {" + nameof(AppId) + "}")]
    [JsonObject("app")]
    public class ApplicationResult
    {
        /// <summary>
        /// Application-Id
        /// </summary>
        [JsonProperty("id")]
        public string AppId { get; set; }

        /// <summary>
        /// Application-Name
        /// </summary>
        [JsonProperty("name")]
        public string AppName { get; set; }

        /// <summary>
        /// Application-Type
        /// </summary>
        [JsonProperty("applicationType")]
        public string AppType { get; set; }

        /// <summary>
        /// State
        /// </summary>
        [JsonProperty("state")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EAppState State { get; set; }

        /// <summary>
        /// Final status
        /// </summary>
        [JsonProperty("finalStatus")]
        public EFinalStatus FinalStatus { get; set; }

        /// <summary>
        /// Progress
        /// </summary>
        [JsonProperty("progress")]
        public float Progess { get; set; }

        /// <summary>
        /// Tracking-URL
        /// </summary>
        [JsonProperty("trackingUrl")]
        public string TrackingUrl { get; set; }

        /// <summary>
        /// Start-Time
        /// </summary>
        [JsonProperty("startedTime")]
        [JsonConverter(typeof(JsonJavaEpochConverter))]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Finish-Time
        /// </summary>
        [JsonProperty("finishedTime")]
        [JsonConverter(typeof(JsonJavaEpochConverter))]
        public DateTime FinishTime { get; set; }

        /// <summary>
        /// AM Host
        /// </summary>
        //[JsonProperty("amHostHttpAddress")]
        //[JsonConverter(typeof(JsonConverter))]
        public YarnNode AmHost { get; set; }

        /// <summary>
        /// AM Host HTTP Address
        /// </summary>
        [JsonProperty("amHostHttpAddress")]
        public string AmHostHttpAddress { get; set; }

        /// <summary>
        /// Aggregate Resource Allocation MB-seconds
        /// </summary>
        [JsonProperty("memorySeconds")]
        public long MbSeconds { get; set; }

        /// <summary>
        /// Aggregate Resource Allocation vcore-seconds
        /// </summary>
        [JsonProperty("vcoreSeconds")]
        public long VcoreSeconds { get; set; }

        public override bool Equals(object obj)
        {
            var result = obj as ApplicationResult;
            return result != null &&
                   AppId == result.AppId &&
                   AppName == result.AppName &&
                   AppType == result.AppType &&
                   State == result.State &&
                   FinalStatus == result.FinalStatus &&
                   Math.Abs(Progess - result.Progess) < 0.01 &&
                   TrackingUrl == result.TrackingUrl &&
                   StartTime == result.StartTime &&
                   FinishTime == result.FinishTime &&
                   EqualityComparer<YarnNode>.Default.Equals(AmHost, result.AmHost) &&
                   AmHostHttpAddress == result.AmHostHttpAddress &&
                   MbSeconds == result.MbSeconds &&
                   VcoreSeconds == result.VcoreSeconds;
        }

        public override int GetHashCode()
        {
            var hashCode = -1000931503;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AppId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AppName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AppType);
            hashCode = hashCode * -1521134295 + State.GetHashCode();
            hashCode = hashCode * -1521134295 + FinalStatus.GetHashCode();
            hashCode = hashCode * -1521134295 + Progess.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TrackingUrl);
            hashCode = hashCode * -1521134295 + StartTime.GetHashCode();
            hashCode = hashCode * -1521134295 + FinishTime.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<YarnNode>.Default.GetHashCode(AmHost);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AmHostHttpAddress);
            hashCode = hashCode * -1521134295 + MbSeconds.GetHashCode();
            hashCode = hashCode * -1521134295 + VcoreSeconds.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Data for application attempts
    /// </summary>
    /// <remarks>
    /// CMD List:       <c>yarn applicationattempt -list &lt;appID&gt;</c>
    /// CMD Details:    <c>yarn applicationattempt -status &lt;appAttemptId&gt;</c>
    /// REST List:      <c>http://controller:8088/ws/v1/cluster/apps/{appid}/appattempts</c>
    /// </remarks>
    [DebuggerDisplay("Attempt {" + nameof(AttemptId) + "}")]
    [JsonObject("appAttempt")]
    public class ApplicationAttemptResult
    {
        /// <summary>
        /// ApplicationAttempt-Id
        /// </summary>
        [JsonProperty("id")]
        public string AttemptId { get; set; }

        /// <summary>
        /// State
        /// </summary>
        public EAppState State { get; set; }

        /// <summary>
        /// AM-Container-Id
        /// </summary>
        [JsonProperty("containerId")]
        public string AmContainerId { get; set; }

        /// <summary>
        /// Tracking-URL
        /// </summary>
        public string TrackingUrl { get; set; }

        /// <summary>
        /// AM Host
        /// </summary>
        public YarnNode AmHost { get; set; }

        /// <summary>
        /// AM Host Node ID
        /// </summary>
        [JsonProperty("nodeId")]
        public string AmHostId { get; set; }

        /// <summary>
        /// Start-Time
        /// </summary>
        [JsonProperty("startTime")]
        [JsonConverter(typeof(JsonJavaEpochConverter))]
        public DateTime StartTime { get; set; }

        public override bool Equals(object obj)
        {
            var result = obj as ApplicationAttemptResult;
            return result != null &&
                   AttemptId == result.AttemptId &&
                   State == result.State &&
                   AmContainerId == result.AmContainerId &&
                   TrackingUrl == result.TrackingUrl &&
                   EqualityComparer<YarnNode>.Default.Equals(AmHost, result.AmHost) &&
                   AmHostId == result.AmHostId &&
                   StartTime == result.StartTime;
        }

        public override int GetHashCode()
        {
            var hashCode = -1199140868;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AttemptId);
            hashCode = hashCode * -1521134295 + State.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AmContainerId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TrackingUrl);
            hashCode = hashCode * -1521134295 + EqualityComparer<YarnNode>.Default.GetHashCode(AmHost);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AmHostId);
            hashCode = hashCode * -1521134295 + StartTime.GetHashCode();
            return hashCode;
        }
    }

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
    /// </remarks>
    [DebuggerDisplay("Container {" + nameof(ContainerId) + "}")]
    [JsonObject("container")]
    public class ContainerResult
    {
        /// <summary>
        /// Container-Id
        /// </summary>
        [JsonProperty("id")]
        public string ContainerId { get; set; }

        /// <summary>
        /// Start Time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Finish Time
        /// </summary>
        public DateTime FinishTime { get; set; }

        /// <summary>
        /// State
        /// </summary>
        [JsonProperty("state")]
        public EContainerState State { get; set; }

        /// <summary>
        /// Host
        /// </summary>
        public YarnNode Host { get; set; }

        /// <summary>
        /// AM Host Node ID
        /// </summary>
        [JsonProperty("nodeId")]
        public string HostId { get; set; }

        /// <summary>
        /// LOG-URL
        /// </summary>
        [JsonProperty("containerLogsLink")]
        public string LogUrl { get; set; }

        /// <summary>
        /// Exit code
        /// </summary>
        [JsonProperty("exitCode")]
        public int ExitCode { get; set; }

        /// <summary>
        /// Diagnostics message for failed containers
        /// </summary>
        [JsonProperty("diagnostics")]
        public string Diagnostics { get; set; }

        /// <summary>
        /// Amount of needed Memory in MB
        /// </summary>
        [JsonProperty("totalMemoryNeededMB")]
        public long MemoryNeeded { get; }

        /// <summary>
        /// Amound of needed VCores
        /// </summary>
        [JsonProperty("totalVCoresNeeded")]
        public long VcoresNeeded { get; }

        public override bool Equals(object obj)
        {
            var result = obj as ContainerResult;
            return result != null &&
                   ContainerId == result.ContainerId &&
                   StartTime == result.StartTime &&
                   FinishTime == result.FinishTime &&
                   State == result.State &&
                   EqualityComparer<YarnNode>.Default.Equals(Host, result.Host) &&
                   HostId == result.HostId &&
                   LogUrl == result.LogUrl &&
                   ExitCode == result.ExitCode &&
                   Diagnostics == result.Diagnostics &&
                   MemoryNeeded == result.MemoryNeeded &&
                   VcoresNeeded == result.VcoresNeeded;
        }

        public override int GetHashCode()
        {
            var hashCode = -524235881;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContainerId);
            hashCode = hashCode * -1521134295 + StartTime.GetHashCode();
            hashCode = hashCode * -1521134295 + FinishTime.GetHashCode();
            hashCode = hashCode * -1521134295 + State.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<YarnNode>.Default.GetHashCode(Host);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HostId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LogUrl);
            hashCode = hashCode * -1521134295 + ExitCode.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Diagnostics);
            hashCode = hashCode * -1521134295 + MemoryNeeded.GetHashCode();
            hashCode = hashCode * -1521134295 + VcoresNeeded.GetHashCode();
            return hashCode;
        }
    }

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
    [DebuggerDisplay("Node {" + nameof(NodeId) + "}")]
    [JsonObject("node")]
    public class NodeResult
    {
        /// <summary>
        /// Node-Id
        /// </summary>
        [JsonProperty("id")]
        public string NodeId { get; set; }

        /// <summary>
        /// Node hostname
        /// </summary>
        [JsonProperty("nodeHostName")]
        public string Hostname { get; set; }

        /// <summary>
        /// Node-State
        /// </summary>
        [JsonProperty("state")]
        public ENodeState NodeState { get; set; }

        /// <summary>
        /// Node-Http-Address
        /// </summary>
        [JsonProperty("nodeHTTPAddress")]
        public string NodeHttpAdd { get; set; }

        /// <summary>
        /// Number-of-Running-Containers
        /// </summary>
        [JsonProperty("numContainers")]
        public int RunningContainerCount { get; set; }

        /// <summary>
        /// Memory-Used in MB
        /// </summary>
        [JsonProperty("usedMemoryMB")]
        public long MemoryUsed { get; set; }

        /// <summary>
        /// Memory-Capacity in MB
        /// </summary>
        [JsonProperty("availMemoryMB")]
        public long MemoryCapacity { get; set; }

        /// <summary>
        /// CPU-Used in vcores
        /// </summary>
        [JsonProperty("usedVirtualCores")]
        public long CpuUsed { get; set; }

        /// <summary>
        /// CPU-Capacity in vcores
        /// </summary>
        [JsonProperty("availableVirtualCores")]
        public long CpuCapacity { get; set; }

        /// <summary>
        /// Health status
        /// </summary>
        [JsonProperty("healthStatus")]
        public string HealthStatus { get; set; }

        /// <summary>
        /// Health Report
        /// </summary>
        [JsonProperty("healthReport")]
        public string HealthReport { get; set; }

        /// <summary>
        /// Last Health update
        /// </summary>
        [JsonProperty("lastHealthUpdate")]
        [JsonConverter(typeof(JsonJavaEpochConverter))]
        public DateTime LastHealthUpdate { get; set; }

        public override bool Equals(object obj)
        {
            var result = obj as NodeResult;
            return result != null &&
                   NodeId == result.NodeId &&
                   Hostname == result.Hostname &&
                   NodeState == result.NodeState &&
                   NodeHttpAdd == result.NodeHttpAdd &&
                   RunningContainerCount == result.RunningContainerCount &&
                   MemoryUsed == result.MemoryUsed &&
                   MemoryCapacity == result.MemoryCapacity &&
                   CpuUsed == result.CpuUsed &&
                   CpuCapacity == result.CpuCapacity &&
                   HealthStatus == result.HealthStatus &&
                   HealthReport == result.HealthReport &&
                   LastHealthUpdate == result.LastHealthUpdate;
        }

        public override int GetHashCode()
        {
            var hashCode = -188306626;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NodeId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Hostname);
            hashCode = hashCode * -1521134295 + NodeState.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NodeHttpAdd);
            hashCode = hashCode * -1521134295 + RunningContainerCount.GetHashCode();
            hashCode = hashCode * -1521134295 + MemoryUsed.GetHashCode();
            hashCode = hashCode * -1521134295 + MemoryCapacity.GetHashCode();
            hashCode = hashCode * -1521134295 + CpuUsed.GetHashCode();
            hashCode = hashCode * -1521134295 + CpuCapacity.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HealthStatus);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HealthReport);
            hashCode = hashCode * -1521134295 + LastHealthUpdate.GetHashCode();
            return hashCode;
        }
    }
}