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
    /// Data for applications
    /// </summary>
    /// <remarks>
    /// CMD List:       <c>yarn application -appStates &lt;states&gt; -list</c>
    /// CMD Details:    <c>yarn application -status &lt;appId&gt;</c>
    /// REST List:      <c>http://controller:8088/ws/v1/cluster/apps</c>
    /// REST Details:   <c>http://controller:8088/ws/v1/cluster/apps/{appid}</c>
    /// </remarks>
    [DebuggerDisplay("Application {" + nameof(AppId) + "}")]
    public class ApplicationResult : IApplicationResult
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
        [JsonIgnore]
        public YarnNode AmHost { get; set; }

        /// <summary>
        /// AM Host HTTP Address
        /// </summary>
        [JsonProperty("amHostHttpAddress")]
        public string AmHostHttpAddress { get; set; }

        /// <summary>
        /// Running containers count
        /// </summary>
        [JsonProperty("runningContainers")]
        public long RunningContainers { get; set; }

        /// <summary>
        /// Allocated Memory in MB
        /// </summary>
        [JsonProperty("allocatedMB")]
        public long AllocatedMb { get; set; }

        /// <summary>
        /// Allocated CPU in VCores
        /// </summary>
        [JsonProperty("allocatedVCores")]
        public long AllocatedVcores { get; set; }

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

        /// <summary>
        /// Preempted Memory in MB
        /// </summary>
        [JsonProperty("preemptedResourceMB")]
        public long PreemptedMb { get; set; }

        /// <summary>
        /// Preempted CPU in VCores
        /// </summary>
        [JsonProperty("preemptedResourceVCores")]
        public long PreemptedVcores { get; set; }

        /// <summary>
        /// Preempted Non-AM container count
        /// </summary>
        [JsonProperty("numNonAMContainerPreempted")]
        public long NonAmContainerPreempted { get; set; }

        /// <summary>
        /// Preempted AM container count
        /// </summary>
        [JsonProperty("numAMContainerPreempted")]
        public long AmContainerPreempted { get; set; }

        /// <summary>
        /// Diagnostics info
        /// </summary>
        [JsonProperty("diagnosticsInfo")]
        public string Diagnostics { get; set; }

        public override bool Equals(object obj)
        {
            var result = obj as ApplicationResult;
            return result != null &&
                   AppId == result.AppId &&
                   AppName == result.AppName &&
                   AppType == result.AppType &&
                   State == result.State &&
                   FinalStatus == result.FinalStatus &&
                   Progess == result.Progess &&
                   TrackingUrl == result.TrackingUrl &&
                   StartTime == result.StartTime &&
                   FinishTime == result.FinishTime &&
                   EqualityComparer<YarnNode>.Default.Equals(AmHost, result.AmHost) &&
                   AmHostHttpAddress == result.AmHostHttpAddress &&
                   RunningContainers == result.RunningContainers &&
                   AllocatedMb == result.AllocatedMb &&
                   AllocatedVcores == result.AllocatedVcores &&
                   MbSeconds == result.MbSeconds &&
                   VcoreSeconds == result.VcoreSeconds &&
                   PreemptedMb == result.PreemptedMb &&
                   PreemptedVcores == result.PreemptedVcores &&
                   NonAmContainerPreempted == result.NonAmContainerPreempted &&
                   AmContainerPreempted == result.AmContainerPreempted;
        }

        public override int GetHashCode()
        {
            var hashCode = -2097537572;
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
            hashCode = hashCode * -1521134295 + RunningContainers.GetHashCode();
            hashCode = hashCode * -1521134295 + AllocatedMb.GetHashCode();
            hashCode = hashCode * -1521134295 + AllocatedVcores.GetHashCode();
            hashCode = hashCode * -1521134295 + MbSeconds.GetHashCode();
            hashCode = hashCode * -1521134295 + VcoreSeconds.GetHashCode();
            hashCode = hashCode * -1521134295 + PreemptedMb.GetHashCode();
            hashCode = hashCode * -1521134295 + PreemptedVcores.GetHashCode();
            hashCode = hashCode * -1521134295 + NonAmContainerPreempted.GetHashCode();
            hashCode = hashCode * -1521134295 + AmContainerPreempted.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Helper class for deserializing application list jsons from hadoop rest api
    /// </summary>
    public class ApplicationListJsonResult
    {
        /// <summary>
        /// The collection with the applications
        /// </summary>
        [JsonProperty("apps")]
        public ApplicationJsonResultCollection Collection { get; set; }
    }

    /// <summary>
    /// Helper class for containing an application list (via json or timeline json)
    /// </summary>
    public class ApplicationJsonResultCollection
    {
        /// <summary>
        /// The application
        /// </summary>
        [JsonProperty("app")]
        public ApplicationResult[] List { get; set; }
    }

    /// <summary>
    /// Helper class for deserializing application details jsons from hadoop rest api
    /// </summary>
    public class ApplicationDetailsJsonResult
    {
        /// <summary>
        /// The application
        /// </summary>
        [JsonProperty("app")]
        public ApplicationResult App { get; set; }
    }
}