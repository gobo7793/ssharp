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
        /// <see cref="YarnApp"/> ID from this attempt
        /// </summary>
        // public string AppId { get; set; }
        public char[] AppIdActual { get; private set; }

        /// <summary>
        /// <see cref="YarnApp"/> ID from this attempt as string, based on <see cref="AppIdActual"/>
        /// </summary>
        [NonSerializable]
        public string AppId
        {
            get { return ModelUtilities.GetCharArrayAsString(AppIdActual); }
            set { ModelUtilities.SetCharArrayOnString(AppIdActual, value); }
        }

        /// <summary>
        /// <see cref="YarnApp"/> from this attempt
        /// </summary>
        [NonSerializable]
        public YarnApp App => Model.Instance.Applications.FirstOrDefault(s => s.AppId == AppId);

        /// <summary>
        /// Attempt ID
        /// </summary>
        // public string AttemptId { get; set; }
        public char[] AttemptIdActual { get; private set; }

        /// <summary>
        /// Attempt ID as string, based on <see cref="AttemptIdActual"/>
        /// </summary>
        [NonSerializable]
        public string AttemptId
        {
            get { return ModelUtilities.GetCharArrayAsString(AttemptIdActual); }
            set { ModelUtilities.SetCharArrayOnString(AttemptIdActual, value); }
        }

        /// <summary>
        /// Current State
        /// </summary>
        public EAppState State { get; set; }

        /// <summary>
        /// ContainerId for ApplicationMaster
        /// </summary>
        // public string AmContainerId { get; set; }
        public char[] AmContainerIdActual { get; private set; }

        /// <summary>
        /// ContainerId for ApplicationMaster as string, based on <see cref="AmContainerIdActual"/>
        /// </summary>
        [NonSerializable]
        public string AmContainerId
        {
            get { return ModelUtilities.GetCharArrayAsString(AmContainerIdActual); }
            set { ModelUtilities.SetCharArrayOnString(AmContainerIdActual, value); }
        }

        /// <summary>
        /// ApplicationMaster Container, null if not available
        /// </summary>
        [NonSerializable]
        public YarnAppContainer AmContainer => Containers.FirstOrDefault(c => c.ContainerId == AmContainerId);

        /// <summary>
        /// <see cref="YarnNode"/> ID the ApplicationMaster is running
        /// </summary>
        // public string AmHostId { get; set; }
        public char[] AmHostIdActual { get; private set; }

        /// <summary>
        /// <see cref="YarnNode"/> ID the ApplicationMaster is running as string, based on <see cref="AmHostIdActual"/>
        /// </summary>
        [NonSerializable]
        public string AmHostId
        {
            get { return ModelUtilities.GetCharArrayAsString(AmHostIdActual); }
            set { ModelUtilities.SetCharArrayOnString(AmHostIdActual, value); }
        }

        /// <summary>
        /// <see cref="YarnNode"/> the ApplicationMaster is running
        /// </summary>
        [NonSerializable]
        public YarnNode AmHost => Model.Instance.Nodes.FirstOrDefault(h => h.NodeId == AmHostId);

        /// <summary>
        /// The tracking url for web UI
        /// </summary>
        // public string TrackingUrl { get; set; }
        public char[] TrackingUrlActual { get; private set; }

        /// <summary>
        /// The tracking url for web UI as string, based on <see cref="TrackingUrlActual"/>
        /// </summary>
        [NonSerializable]
        public string TrackingUrl
        {
            get { return ModelUtilities.GetCharArrayAsString(TrackingUrlActual); }
            set { ModelUtilities.SetCharArrayOnString(TrackingUrlActual, value); }
        }

        /// <summary>
        /// Starting Time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Diagnostics message for failed containers
        /// </summary>
        // public string Diagnostics { get; set; }
        public char[] DiagnosticsActual { get; private set; }

        /// <summary>
        /// Diagnostics message for failed containers as string, based on <see cref="DiagnosticsActual"/>
        /// </summary>
        [NonSerializable]
        public string Diagnostics
        {
            get { return ModelUtilities.GetCharArrayAsString(DiagnosticsActual); }
            set { ModelUtilities.SetCharArrayOnString(DiagnosticsActual, value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new empty <see cref="YarnAppAttempt"/>
        /// </summary>
        public YarnAppAttempt()
        {
            Containers = new List<YarnAppContainer>();
            State = EAppState.NotStartedYet;

            AppIdActual = new char[ModelSettings.AppIdLength];
            AttemptIdActual = new char[ModelSettings.AppAttemptIdLength];
            AmContainerIdActual = new char[ModelSettings.ContainerIdLength];
            AmHostIdActual = new char[ModelSettings.NodeIdLength];
            TrackingUrlActual = new char[ModelSettings.TrackingUrlLength];
            DiagnosticsActual = new char[ModelSettings.DiagnosticsLength];

            IsSelfMonitoring = false;
        }

        #endregion

        #region IYarnReadable Methods

        /// <summary>
        /// Parser to monitoring data from cluster
        /// </summary>
        [NonSerializable]
        public IHadoopParser Parser => Model.Instance.UsingMonitoringParser;

        /// <summary>
        /// Indicates if the data is collected and parsed by the component itself
        /// </summary>
        public bool IsSelfMonitoring { get; set; }

        /// <summary>
        /// The <see cref="IParsedComponent"/> to get the status on the previous step
        /// </summary>
        public IParsedComponent PreviousParsedComponent { get; set; }

        /// <summary>
        /// The <see cref="IParsedComponent"/> to get the status on the current step
        /// </summary>
        public IParsedComponent CurrentParsedComponent { get; set; }

        /// <summary>
        /// S# constraints for the oracle based on requirement for the SuT
        /// </summary>
        [Hidden(HideElements = true)]
        public Func<bool>[] SutConstraints => new Func<bool>[]
        {
            // 3) configuration will be updated
            () =>
            {
                if(String.IsNullOrWhiteSpace(AttemptId))
                    return true;

                OutputUtilities.PrintTestConstraint("configuration will be updated", GetId());
                if(State == EAppState.RUNNING)
                    return AmHost?.State == ENodeState.RUNNING;
                return true;
            },
        };

        /// <summary>
        /// Constraints to check the requirements of the test suite itself
        /// </summary>
        [Hidden(HideElements = true)]
        public Func<bool>[] TestConstraints => new Func<bool>[]
        {
            // 5 current state is detected and saved
            () =>
            {
                if(String.IsNullOrWhiteSpace(AttemptId))
                    return true;

                OutputUtilities.PrintTestConstraint("current state is detected and saved", GetId());
                var prev = PreviousParsedComponent as IAppAttemptResult;
                var curr = CurrentParsedComponent as IAppAttemptResult;
                if((prev == null && curr != null) ||
                   (prev != null && curr != null && !ReferenceEquals(curr, prev)))
                {
                    return State == curr.State &&
                           AmContainerId == curr.AmContainerId &&
                           (String.IsNullOrWhiteSpace(AmHostId) && curr.AmHost == null ||
                            AmHostId == curr.AmHost.NodeId) &&
                           String.IsNullOrWhiteSpace(TrackingUrl) == String.IsNullOrWhiteSpace(curr.TrackingUrl) ||
                           curr.TrackingUrl.StartsWith(TrackingUrl) &&
                           StartTime == curr.StartTime &&
                           String.IsNullOrWhiteSpace(Diagnostics) == String.IsNullOrWhiteSpace(curr.Diagnostics) ||
                           curr.Diagnostics.StartsWith(Diagnostics);
                }

                return false;
            },
        };

        /// <summary>
        /// Returns the ID of the component
        /// </summary>
        public string GetId()
        {
            return AttemptId;
        }

        /// <summary>
        /// Monitors the current state from Hadoop
        /// </summary>
        public void MonitorStatus()
        {
            if(IsSelfMonitoring)
            {
                var parsed = Parser.ParseAppAttemptDetails(AttemptId);
                if(parsed != null)
                    SetStatus(parsed);
            }

            if(Containers.Any())
            {
                var parsedContainers = Parser.ParseContainerList(AttemptId);
                foreach(var container in Containers) container.CleanContainer();
                foreach(var parsed in parsedContainers)
                {
                    var container = Containers.FirstOrDefault(c => String.IsNullOrWhiteSpace(c.ContainerId));
                    if(container == null)
                        throw new OutOfMemoryException($"Failed to allocate container {parsed.ContainerId}: No more containers available.");

                    container.AppAttemptId = AttemptId;
                    container.IsSelfMonitoring = IsSelfMonitoring;
                    if(IsSelfMonitoring)
                        container.ContainerId = parsed.ContainerId;
                    else
                        container.SetStatus(parsed);
                }
            }
        }

        /// <summary>
        /// Sets the status based on the given <see cref="IParsedComponent"/>
        /// </summary>
        /// <param name="parsed">The parsed component</param>
        public void SetStatus(IParsedComponent parsed)
        {
            PreviousParsedComponent = CurrentParsedComponent;
            CurrentParsedComponent = parsed;

            var attempt = parsed as IAppAttemptResult;
            AttemptId = attempt.AttemptId;
            State = attempt.State;
            AmContainerId = attempt.AmContainerId;
            //AmHost = attempt.AmHost;
            if(attempt.AmHost != null)
                AmHostId = attempt.AmHost.NodeId;
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
            if(IsSelfMonitoring && !String.IsNullOrWhiteSpace(AttemptId))
                MonitorStatus();
        }

        #endregion
    }
}