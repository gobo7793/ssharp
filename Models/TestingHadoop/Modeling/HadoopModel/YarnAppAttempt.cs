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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// Execution attempt of a <see cref="YarnApp"/>
    /// </summary>
    [DebuggerDisplay("Attempt {" + nameof(AttemptId) + "}")]
    public class YarnAppAttempt : Component, IYarnReadable
    {

        #region Properties

        /// <summary>
        /// Running Containers
        /// </summary>
        public List<YarnAppContainer> Containers { get; }

        /// <summary>
        /// <see cref="YarnApp"/> from this attempt
        /// </summary>
        public YarnApp App { get; set; }

        /// <summary>
        /// Attempt ID
        /// </summary>
        public string AttemptId { get; set; }

        /// <summary>
        /// Current State
        /// </summary>
        public EAppState State { get; set; }

        /// <summary>
        /// ContainerId for ApplicationMaster
        /// </summary>
        public string AmContainerId { get; set; }

        /// <summary>
        /// ApplicationMaster Container, null if not available
        /// </summary>
        public YarnAppContainer AmContainer => Containers.FirstOrDefault(c => c.ContainerId == AmContainerId);

        /// <summary>
        /// <see cref="YarnNode"/> the ApplicationMaster is running
        /// </summary>
        public YarnNode AmHost { get; set; }

        /// <summary>
        /// The tracking url for web UI
        /// </summary>
        public string TrackingUrl { get; set; }

        /// <summary>
        /// Starting Time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Diagnostics message for failed containers
        /// </summary>
        public string Diagnostics { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new empty <see cref="YarnAppAttempt"/>
        /// </summary>
        public YarnAppAttempt()
        {
            Containers = new List<YarnAppContainer>();
            State = EAppState.NotStartedYet;

            IsRequireDetailsParsing = true;
        }

        /// <summary>
        /// Initializes a new <see cref="YarnAppAttempt"/>
        /// </summary>
        /// <param name="app"><see cref="YarnApp"/> from this attempt</param>
        /// <param name="parser">Parser to monitoring data from cluster</param>
        public YarnAppAttempt(YarnApp app, IHadoopParser parser) : this()
        {
            App = app;
            Parser = parser;
        }

        #endregion

        #region IYarnReadable Methods

        /// <summary>
        /// Parser to monitoring data from cluster
        /// </summary>
        public IHadoopParser Parser { get; set; }

        /// <summary>
        /// Indicates if details parsing is required for full informations
        /// </summary>
        public bool IsRequireDetailsParsing { get; set; }

        /// <summary>
        /// Reads the current state from Hadoop
        /// </summary>
        public void ReadStatus()
        {
            if(IsRequireDetailsParsing)
            {
                var parsed = Parser.ParseAppAttemptDetails(AttemptId);
                if(parsed != null)
                    SetStatus(parsed);
            }

            var parsedContainers = Parser.ParseContainerList(AttemptId);
            foreach(var parsed in parsedContainers)
            {
                var container = Containers.FirstOrDefault(c => c.ContainerId == parsed.ContainerId) ??
                              Containers.FirstOrDefault(c => String.IsNullOrWhiteSpace(c.ContainerId));
                if(container == null)
                    throw new OutOfMemoryException("No more containers available! Try to initialize more containers.");

                if(IsRequireDetailsParsing)
                {
                    container.ContainerId = parsed.ContainerId;
                }
                else
                {
                    container.SetStatus(parsed);
                    container.IsRequireDetailsParsing = IsRequireDetailsParsing;
                }
            }
        }

        /// <summary>
        /// Sets the status based on the parsed component
        /// </summary>
        /// <param name="parsed">The parsed component</param>
        public void SetStatus(IParsedComponent parsed)
        {
            var attempt = parsed as IAppAttemptResult;
            AttemptId = attempt.AttemptId;
            State = attempt.State;
            AmContainerId = attempt.AmContainerId;
            AmHost = attempt.AmHost;
            TrackingUrl = attempt.TrackingUrl;
            StartTime = attempt.StartTime;
            Diagnostics = attempt.Diagnostics;
        }

        /// <summary>
        /// Returns the current status as comma seperated string
        /// </summary>
        /// <returns>The status as string</returns>
        public string StatusAsString()
        {
            var type = GetType();
            var properties = new List<PropertyInfo>(type.GetProperties());
            var status = String.Empty;
            foreach(var p in properties)
            {
                var value = String.Empty;
                if(p.PropertyType.IsValueType || p.PropertyType.Name == typeof(String).Name)
                    value = p.GetValue(this)?.ToString();
                else if(typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                {
                    var propVal = p.GetValue(this) as IEnumerable;
                    if(propVal != null)
                    {
                        var cnt = 0;
                        foreach(var unused in propVal) cnt++;
                        value = cnt.ToString();
                    }
                }
                else
                    value = p.GetValue(this)?.GetType().Name;
                status += $"{p.Name}={value},";
            }

            return status;
        }

        #endregion

        #region Methods

        public override void Update()
        {
            if(IsRequireDetailsParsing && !String.IsNullOrWhiteSpace(AttemptId))
                ReadStatus();
        }

        #endregion
    }
}