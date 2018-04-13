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
    /// Execution Container for <see cref="AppAttempt"/>s
    /// </summary>
    [DebuggerDisplay("Container {" + nameof(ContainerId) + "}")]
    public class YarnAppContainer : Component, IYarnReadable
    {
        #region Properties

        /// <summary>
        /// Current State
        /// </summary>
        public EContainerState State { get; set; }

        /// <summary>
        /// Container ID
        /// </summary>
        // public string ContainerId { get; set; }
        public char[] ContainerIdActual { get; private set; }

        /// <summary>
        /// Container ID as string, based on <see cref="ContainerIdActual"/>
        /// </summary>
        [NonSerializable]
        public string ContainerId
        {
            get { return ModelUtilities.GetCharArrayAsString(ContainerIdActual); }
            set { ModelUtilities.SetCharArrayOnString(ContainerIdActual, value); }
        }

        /// <summary>
        /// Starting Time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Ending Time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// <see cref="YarnNode"/> ID running this container
        /// </summary>
        // public string HostId { get; set; }
        public char[] HostIdActual { get; private set; }

        /// <summary>
        /// <see cref="YarnNode"/> ID running this container as string, based on <see cref="HostIdActual"/>
        /// </summary>
        [NonSerializable]
        public string HostId
        {
            get { return ModelUtilities.GetCharArrayAsString(HostIdActual); }
            set { ModelUtilities.SetCharArrayOnString(HostIdActual, value); }
        }

        /// <summary>
        /// <see cref="YarnNode"/> running this container
        /// </summary>
        [NonSerializable]
        public YarnNode Host => Model.Instance.Nodes.FirstOrDefault(h => h.NodeId == HostId);

        /// <summary>
        /// <see cref="YarnAppAttempt"/> ID running in this container
        /// </summary>
        // public string AppAttemptId { get; set; }
        public char[] AppAttemptIdActual { get; private set; }

        /// <summary>
        /// <see cref="YarnAppAttempt"/> ID running in this container as string, based on <see cref="AppAttemptIdActual"/>
        /// </summary>
        [NonSerializable]
        public string AppAttemptId
        {
            get { return ModelUtilities.GetCharArrayAsString(AppAttemptIdActual); }
            set { ModelUtilities.SetCharArrayOnString(AppAttemptIdActual, value); }
        }

        /// <summary>
        /// <see cref="YarnAppAttempt"/> running in this container
        /// </summary>
        [NonSerializable]
        public YarnAppAttempt AppAttempt => Model.Instance.AppAttempts.FirstOrDefault(a => a.AttemptId == AppAttemptId);

        /// <summary>
        /// Priority of the container
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Exit code of the container
        /// </summary>
        public int ExitCode { get; set; }

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

        /// <summary>
        /// Amount of needed/allocated Memory in MB
        /// </summary>
        public long AllocatedMemory { get; set; }

        /// <summary>
        /// Amound of needed/allocated VCores
        /// </summary>
        public long AllocatedVcores { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new empty <see cref="YarnAppContainer"/>
        /// </summary>
        public YarnAppContainer()
        {
            ContainerIdActual = new char[Model.ContainerIdLength];
            HostIdActual = new char[Model.NodeIdLength];
            AppAttemptIdActual = new char[Model.AppAttemptIdLength];
            DiagnosticsActual = new char[Model.DiagnosticsLength];

            IsSelfMonitoring = false;

            CleanContainer();
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
            if(!IsSelfMonitoring)
                return;

            var parsed = Parser.ParseContainerDetails(ContainerId);
            if(parsed != null)
                SetStatus(parsed);
        }

        /// <summary>
        /// Sets the status based on the given <see cref="IParsedComponent"/>
        /// </summary>
        /// <param name="parsed">The parsed component</param>
        public void SetStatus(IParsedComponent parsed)
        {
            var container = parsed as IContainerResult;
            ContainerId = container.ContainerId;
            StartTime = container.StartTime;
            EndTime = container.FinishTime;
            State = container.State;
            //Host = container.Host;
            if(container.Host != null)
                HostId = container.Host.NodeId;
            Priority = container.Priority;
            ExitCode = container.ExitCode;
            Diagnostics = container.Diagnostics;
            AllocatedMemory = container.MemoryNeeded;
            AllocatedVcores = container.VcoresNeeded;
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
            // 2) no workload is allocated to an inactive/defect/disconnected node
            // 3) configuration will be updated
            () =>
            {
                if(State == EContainerState.RUNNING)
                    return Host?.State == ENodeState.RUNNING;
                return true;
            },
        };

        #endregion

        #region Methods

        public override void Update()
        {
            if(IsSelfMonitoring && !String.IsNullOrWhiteSpace(ContainerId))
                MonitorStatus();
        }

        /// <summary>
        /// Cleans all container data
        /// </summary>
        public void CleanContainer()
        {
            ContainerId = String.Empty;
            StartTime = DateTime.MinValue;
            EndTime = DateTime.MinValue;
            State = EContainerState.None;
            HostId = String.Empty;
            Priority = 0;
            ExitCode = 0;
            Diagnostics = String.Empty;
            AllocatedMemory = 0;
            AllocatedVcores = 0;
        }

        #endregion
    }
}