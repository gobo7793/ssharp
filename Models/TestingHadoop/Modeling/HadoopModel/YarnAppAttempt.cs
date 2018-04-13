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

            AppIdActual = new char[Model.AppIdLength];
            AttemptIdActual = new char[Model.AppAttemptIdLength];
            AmContainerIdActual = new char[Model.ContainerIdLength];
            AmHostIdActual = new char[Model.NodeIdLength];
            TrackingUrlActual = new char[Model.TrackingUrlLength];
            DiagnosticsActual = new char[Model.DiagnosticsLength];

            IsSelfMonitoring = false;
        }

        #endregion

        #region IYarnReadable Methods

        /// <summary>
        /// Parser to monitoring data from cluster
        /// </summary>
        [NonSerializable]
        public IHadoopParser Parser => Model.UsingMonitoringParser;

        /// <summary>
        /// Indicates if the data is collected and parsed by the component itself
        /// </summary>
        public bool IsSelfMonitoring { get; set; }

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

            var parsedContainers = Parser.ParseContainerList(AttemptId);
            foreach(var container in Containers) container.CleanContainer();
            foreach(var parsed in parsedContainers)
            {
                var container = Containers.FirstOrDefault(c => String.IsNullOrWhiteSpace(c.ContainerId));
                if(container == null)
                    throw new OutOfMemoryException("No more containers available! Try to initialize more containers.");

                container.AppAttemptId = AttemptId;
                if(IsSelfMonitoring)
                    container.ContainerId = parsed.ContainerId;
                else
                    container.SetStatus(parsed);
                container.IsSelfMonitoring = IsSelfMonitoring;
            }
        }

        /// <summary>
        /// Sets the status based on the given <see cref="IParsedComponent"/>
        /// </summary>
        /// <param name="parsed">The parsed component</param>
        public void SetStatus(IParsedComponent parsed)
        {
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

        /// <summary>
        /// S# analysis/DCCA constraints for the oracle
        /// </summary>
        [Hidden(HideElements = true)]
        public Func<bool>[] Constraints => new Func<bool>[]
        {
            // 3) configuration will be updated
            () =>
            {
                if(State == EAppState.RUNNING)
                    return AmHost?.State == ENodeState.RUNNING;
                return true;
            },
        };

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