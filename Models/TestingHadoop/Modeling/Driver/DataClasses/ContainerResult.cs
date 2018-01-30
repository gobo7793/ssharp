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
    [DebuggerDisplay("Container {" + nameof(ContainerId) + "}")]
    public class ContainerResult : IContainerResult
    {
        /// <summary>
        /// Container-Id
        /// </summary>
        [JsonProperty("id")]
        public string ContainerId { get; set; }

        [JsonProperty("containerId")]
        private string ContainerIdTl
        {
            set { ContainerId = value; }
            get { return ContainerId; }
        }

        /// <summary>
        /// Start Time
        /// </summary>
        [JsonProperty("startedTime")]
        [JsonConverter(typeof(JsonJavaEpochConverter))]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Finish Time
        /// </summary>
        [JsonProperty("finishedTime")]
        [JsonConverter(typeof(JsonJavaEpochConverter))]
        public DateTime FinishTime { get; set; }

        /// <summary>
        /// State
        /// </summary>
        [JsonProperty("state")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EContainerState State { get; set; }

        [JsonProperty("containerState")]
        [JsonConverter(typeof(StringEnumConverter))]
        private EContainerState StateTl
        {
            set { State = value; }
            get { return State; }
        }

        /// <summary>
        /// Host
        /// </summary>
        [JsonIgnore]
        public YarnNode Host { get; set; }

        /// <summary>
        /// AM Host Node ID
        /// </summary>
        [JsonProperty("nodeId")]
        public string HostId { get; set; }

        [JsonProperty("assignedNodeId")]
        private string HostIdTl
        {
            set { HostId = value; }
            get { return HostId; }
        }

        /// <summary>
        /// LOG-URL
        /// </summary>
        [JsonProperty("containerLogsLink")]
        public string LogUrl { get; set; }

        [JsonProperty("logUrl")]
        private string LogUrlTl
        {
            set { LogUrl = value; }
            get { return LogUrl; }
        }

        /// <summary>
        /// Priority of container
        /// </summary>
        [JsonProperty("priority")]
        public int Priority { get; set; }

        /// <summary>
        /// Exit code
        /// </summary>
        [JsonProperty("exitCode")]
        public int ExitCode { get; set; }

        [JsonProperty("containerExitStatus")]
        private int ExitCodeTl
        {
            set { ExitCode = value; }
            get { return ExitCode; }
        }

        /// <summary>
        /// Diagnostics message for failed containers
        /// </summary>
        [JsonProperty("diagnostics")]
        public string Diagnostics { get; set; }

        [JsonProperty("diagnosticsInfo")]
        private string DiagnosticsTl
        {
            set { Diagnostics = value; }
            get { return Diagnostics; }
        }

        /// <summary>
        /// Amount of needed/allocated Memory in MB
        /// </summary>
        [JsonProperty("totalMemoryNeededMB")]
        public long MemoryNeeded { get; set; }

        [JsonProperty("allocatedMB")]
        private int MemoryTl
        {
            set { MemoryNeeded = value; }
            get { return (int)MemoryNeeded; }
        }

        /// <summary>
        /// Amound of needed/allocated VCores
        /// </summary>
        [JsonProperty("totalVCoresNeeded")]
        public long VcoresNeeded { get; set; }

        [JsonProperty("allocatedVCores")]
        private int VcoresTl
        {
            set { VcoresNeeded = value; }
            get { return (int)VcoresNeeded; }
        }

        public override bool Equals(object obj)
        {
            var result = obj as ContainerResult;
            return result != null &&
                   ContainerId == result.ContainerId &&
                   StartTime == result.StartTime &&
                   FinishTime == result.FinishTime &&
                   State == result.State &&
                   HostId == result.HostId &&
                   LogUrl == result.LogUrl &&
                   Priority == result.Priority &&
                   ExitCode == result.ExitCode &&
                   Diagnostics == result.Diagnostics &&
                   MemoryNeeded == result.MemoryNeeded &&
                   VcoresNeeded == result.VcoresNeeded;
        }

        public override int GetHashCode()
        {
            var hashCode = 1415456545;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ContainerId);
            hashCode = hashCode * -1521134295 + StartTime.GetHashCode();
            hashCode = hashCode * -1521134295 + FinishTime.GetHashCode();
            hashCode = hashCode * -1521134295 + State.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HostId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LogUrl);
            hashCode = hashCode * -1521134295 + Priority.GetHashCode();
            hashCode = hashCode * -1521134295 + ExitCode.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Diagnostics);
            hashCode = hashCode * -1521134295 + MemoryNeeded.GetHashCode();
            hashCode = hashCode * -1521134295 + VcoresNeeded.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Helper class for deserializing app container list jsons from hadoop rest api
    /// </summary>
    public class JsonContainerListResult
    {
        /// <summary>
        /// The collection with the app attempts
        /// </summary>
        [JsonProperty("containers")]
        public JsonContainerResultCollection Collection { get; set; }
    }

    /// <summary>
    /// Helper class for containing the app attempt list (via json or timeline json)
    /// </summary>
    public class JsonContainerResultCollection
    {
        /// <summary>
        /// The app attempt list
        /// </summary>
        [JsonProperty("container")]
        public ContainerResult[] List { get; set; }
    }

    /// <summary>
    /// Helper class for deserializing container details jsons from hadoop rest api
    /// </summary>
    public class ContainerDetailsJsonResult
    {
        /// <summary>
        /// The container
        /// </summary>
        [JsonProperty("container")]
        public ContainerResult Container { get; set; }
    }
}