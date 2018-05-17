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

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.DataClasses
{
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
    public class NodeResult : INodeResult
    {
        private long _MemCap = -1;
        private long _MemAvail = -1;
        private long _CpuCap = -1;
        private long _CpuAvail = -1;

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
        [JsonConverter(typeof(StringEnumConverter))]
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
        /// Available memory in MB
        /// </summary>
        [JsonProperty("availMemoryMB")]
        public long MemoryAvailable
        {
            get
            {
                if(_MemAvail < 0)
                    _MemAvail = _MemCap - MemoryUsed;
                return _MemAvail;
            }
            set { _MemAvail = value; }
        }

        /// <summary>
        /// Memory-Capacity in MB
        /// </summary>
        [JsonIgnore]
        public long MemoryCapacity
        {
            get
            {
                if(_MemCap < 0)
                    _MemCap = _MemAvail + MemoryUsed;
                return _MemCap;
            }
            set { _MemCap = value; }
        }

        /// <summary>
        /// CPU-Used in vcores
        /// </summary>
        [JsonProperty("usedVirtualCores")]
        public long CpuUsed { get; set; }

        /// <summary>
        /// Available CPU in vcores
        /// </summary>
        [JsonProperty("availableVirtualCores")]
        public long CpuAvailable
        {
            get
            {
                if(_CpuAvail < 0)
                    _CpuAvail = _CpuCap - CpuUsed;
                return _CpuAvail;
            }
            set { _CpuAvail = value; }
        }

        /// <summary>
        /// CPU-Capacity in vcores
        /// </summary>
        [JsonIgnore]
        public long CpuCapacity
        {
            get
            {
                if(_CpuCap < 0)
                    _CpuCap = _CpuAvail + CpuUsed;
                return _CpuCap;
            }
            set { _CpuCap = value; }
        }

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
                   MemoryAvailable == result.MemoryAvailable &&
                   MemoryCapacity == result.MemoryCapacity &&
                   CpuUsed == result.CpuUsed &&
                   CpuAvailable == result.CpuAvailable &&
                   CpuCapacity == result.CpuCapacity &&
                   HealthStatus == result.HealthStatus &&
                   HealthReport == result.HealthReport &&
                   LastHealthUpdate == result.LastHealthUpdate;
        }

        public override int GetHashCode()
        {
            var hashCode = -892694673;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NodeId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Hostname);
            hashCode = hashCode * -1521134295 + NodeState.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NodeHttpAdd);
            hashCode = hashCode * -1521134295 + RunningContainerCount.GetHashCode();
            hashCode = hashCode * -1521134295 + MemoryUsed.GetHashCode();
            hashCode = hashCode * -1521134295 + MemoryAvailable.GetHashCode();
            hashCode = hashCode * -1521134295 + MemoryCapacity.GetHashCode();
            hashCode = hashCode * -1521134295 + CpuUsed.GetHashCode();
            hashCode = hashCode * -1521134295 + CpuAvailable.GetHashCode();
            hashCode = hashCode * -1521134295 + CpuCapacity.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HealthStatus);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HealthReport);
            hashCode = hashCode * -1521134295 + LastHealthUpdate.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Helper class for deserializing node list jsons from hadoop rest api
    /// </summary>
    public class NodeListJsonResult
    {
        /// <summary>
        /// The collection with the nodes
        /// </summary>
        [JsonProperty("nodes")]
        public NodeJsonResultCollection Collection { get; set; }
    }

    /// <summary>
    /// Helper class for containing the nodes list (via json)
    /// </summary>
    public class NodeJsonResultCollection
    {
        /// <summary>
        /// The node list
        /// </summary>
        [JsonProperty("node")]
        public NodeResult[] List { get; set; }
    }

    /// <summary>
    /// Helper class for deserializing node details jsons from hadoop rest api
    /// </summary>
    public class NodeDetailsJsonResult
    {
        /// <summary>
        /// The node
        /// </summary>
        [JsonProperty("node")]
        public NodeResult Node { get; set; }
    }
}