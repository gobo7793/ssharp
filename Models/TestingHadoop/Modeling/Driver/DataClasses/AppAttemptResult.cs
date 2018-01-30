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
    /// Data for application attempts
    /// </summary>
    /// <remarks>
    /// CMD List:       <c>yarn applicationattempt -list &lt;appID&gt;</c>
    /// CMD Details:    <c>yarn applicationattempt -status &lt;appAttemptId&gt;</c>
    /// REST List:      <c>http://controller:8088/ws/v1/cluster/apps/{appid}/appattempts</c>
    /// REST TL List:   <c>http://controller:8188/ws/v1/applicationhistory/apps/{appid}/appattempts</c>
    /// REST TL Details:<c>http://controller:8188/ws/v1/applicationhistory/apps/{appid}/appattempts/{appattemptid}</c>
    /// </remarks>
    [DebuggerDisplay("Attempt {" + nameof(AttemptId) + "}")]
    public class AppAttemptResult : IAppAttemptResult
    {
        /// <summary>
        /// ApplicationAttempt-Id
        /// </summary>
        [JsonProperty("id")]
        public string AttemptId { get; set; }

        [JsonProperty("appAttemptId")]
        private string AttemptIdTl
        {
            set { AttemptId = value; }
            get { return AttemptId; }
        }

        /// <summary>
        /// State
        /// </summary>
        [JsonProperty("appAttemptState")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EAppState State { get; set; }

        /// <summary>
        /// AM-Container-Id
        /// </summary>
        [JsonProperty("containerId")]
        public string AmContainerId { get; set; }

        [JsonProperty("amContainerId")]
        private string AmContainerIdTl
        {
            set { AmContainerId = value; }
            get { return AmContainerId; }
        }

        /// <summary>
        /// Tracking-URL
        /// </summary>
        [JsonProperty("trackingUrl")]
        public string TrackingUrl { get; set; }

        /// <summary>
        /// AM Host
        /// </summary>
        [JsonIgnore]
        public YarnNode AmHost { get; set; }

        /// <summary>
        /// AM Host HTTP Address
        /// </summary>
        [JsonProperty("nodeHttpAddress")]
        public string AmHostHttpAddress { get; set; }

        /// <summary>
        /// AM Host Node ID
        /// </summary>
        [JsonProperty("nodeId")]
        public string AmHostId { get; set; }

        [JsonProperty("host")]
        private string AmHostTl
        {
            set { AmHostId = value; }
            get { return AmHostId; }
        }

        /// <summary>
        /// Start-Time
        /// </summary>
        [JsonProperty("startTime")]
        [JsonConverter(typeof(JsonJavaEpochConverter))]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Diagnostics info
        /// </summary>
        [JsonProperty("diagnosticsInfo")]
        public string Diagnostics { get; set; }

        /// <summary>
        /// Logs URL
        /// </summary>
        [JsonProperty("logsLink")]
        public string LogsUrl { get; set; }

        public override bool Equals(object obj)
        {
            var result = obj as AppAttemptResult;
            return result != null &&
                   AttemptId == result.AttemptId &&
                   State == result.State &&
                   AmContainerId == result.AmContainerId &&
                   TrackingUrl == result.TrackingUrl &&
                   EqualityComparer<YarnNode>.Default.Equals(AmHost, result.AmHost) &&
                   AmHostHttpAddress == result.AmHostHttpAddress &&
                   AmHostId == result.AmHostId &&
                   StartTime == result.StartTime &&
                   LogsUrl == result.LogsUrl;
        }

        public override int GetHashCode()
        {
            var hashCode = 1494292438;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AttemptId);
            hashCode = hashCode * -1521134295 + State.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AmContainerId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TrackingUrl);
            hashCode = hashCode * -1521134295 + EqualityComparer<YarnNode>.Default.GetHashCode(AmHost);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AmHostHttpAddress);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AmHostId);
            hashCode = hashCode * -1521134295 + StartTime.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LogsUrl);
            return hashCode;
        }
    }

    /// <summary>
    /// Helper class for deserializing app attempt list jsons from hadoop rest api
    /// </summary>
    public class JsonAppAttemptListResult
    {
        /// <summary>
        /// The collection with the app attempts
        /// </summary>
        [JsonProperty("appAttempts")]
        public JsonAppAttemptResultCollection Collection { get; set; }
    }

    /// <summary>
    /// Helper class for containing the app attempt list (via json or timeline json)
    /// </summary>
    public class JsonAppAttemptResultCollection
    {
        /// <summary>
        /// The app attempt list
        /// </summary>
        [JsonProperty("appAttempt")]
        public AppAttemptResult[] List { get; set; }
    }
}