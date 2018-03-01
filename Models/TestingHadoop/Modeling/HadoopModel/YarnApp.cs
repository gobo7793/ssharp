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
using ISSE.SafetyChecking.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// Application/Job to run on the Hadoop cluster
    /// </summary>
    [DebuggerDisplay("Application {" + nameof(AppId) + "}")]
    public class YarnApp : Component, IYarnReadable
    {

        #region Faults

        /// <summary>
        /// App will be killed
        /// </summary>
        public readonly Fault KillApp = new PermanentFault();

        #endregion

        #region Properties

        /// <summary>
        /// The connector to use for fault handling
        /// </summary>
        public IHadoopConnector FaultConnector { get; set; }

        /// <summary>
        /// <see cref="YarnAppAttempt"/> for this <see cref="YarnApp"/>
        /// </summary>
        public List<YarnAppAttempt> Attempts { get; }

        /// <summary>
        /// Starting <see cref="Client"/> of this app
        /// </summary>
        public Client StartingClient { get; set; }

        /// <summary>
        /// Current state
        /// </summary>
        public EAppState State { get; set; }

        /// <summary>
        /// Final status
        /// </summary>
        public EFinalStatus FinalStatus { get; set; }

        /// <summary>
        /// Name of the app
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ID of the app
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Starting Time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Ending Time, <see cref="DateTime.MinValue"/> if running
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Main ApplicationMaster Host
        /// </summary>
        public YarnNode AmHost { get; set; }

        /// <summary>
        /// Allocated Memory in MB
        /// </summary>
        public long AllocatedMb { get; set; }

        /// <summary>
        /// Allocated CPU in VCores
        /// </summary>
        public long AllocatedVcores { get; set; }

        /// <summary>
        /// Aggregate Resource Allocation MB-seconds
        /// </summary>
        public long MbSeconds { get; set; }

        /// <summary>
        /// Aggregate Resource Allocation vcore-seconds
        /// </summary>
        public long VcoreSeconds { get; set; }

        /// <summary>
        /// Preempted Memory in MB
        /// </summary>
        public long PreemptedMb { get; set; }

        /// <summary>
        /// Preempted CPU in VCores
        /// </summary>
        public long PreemptedVcores { get; set; }

        /// <summary>
        /// Preempted Non-AM container count
        /// </summary>
        public long NonAmContainerPreempted { get; set; }

        /// <summary>
        /// Preempted AM container count
        /// </summary>
        public long AmContainerPreempted { get; set; }

        /// <summary>
        /// Current Progress
        /// </summary>
        public float Progress { get; set; }

        /// <summary>
        /// The tracking url for web UI
        /// </summary>
        public string TrackingUrl { get; set; }

        /// <summary>
        /// The current running container count on the cluster
        /// </summary>
        public long CurrentRunningContainers { get; set; }

        /// <summary>
        /// Diagnostics message for failed containers
        /// </summary>
        public string Diagnostics { get; set; }

        /// <summary>
        /// Indicates that the app can be killed
        /// </summary>
        //public bool IsKillable => State != EAppState.None &&
        //                          (State & (EAppState.FAILED | EAppState.FINISHED | EAppState.KILLED | EAppState.NotStartedYet)) ==
        //                          EAppState.None;
        public bool IsKillable => FinalStatus == EFinalStatus.UNDEFINED;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new empty <see cref="YarnApp"/>
        /// </summary>
        public YarnApp()
        {
            Attempts = new List<YarnAppAttempt>();
            State = EAppState.NotStartedYet;
            FinalStatus = EFinalStatus.None;

            IsSelfMonitoring = true;
        }

        /// <summary>
        /// Initializes a new <see cref="YarnApp"/>
        /// </summary>
        /// <param name="faultConnector">The connector to use for fault handling</param>
        /// <param name="startingClient">Starting <see cref="Client"/> of this app</param>
        /// <param name="parser">Parser to monitoring data from cluster</param>
        public YarnApp(IHadoopConnector faultConnector, Client startingClient, IHadoopParser parser) : this()
        {
            FaultConnector = faultConnector;
            StartingClient = startingClient;
            Parser = parser;
        }

        #endregion

        #region IYarnReadable Methods

        /// <summary>
        /// Parser to monitoring data from cluster
        /// </summary>
        public IHadoopParser Parser { get; set; }

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
                var parsed = Parser.ParseAppDetails(AppId);
                if(parsed != null)
                    SetStatus(parsed);
            }

            var parsedAttempts = Parser.ParseAppAttemptList(AppId);
            foreach(var parsed in parsedAttempts)
            {
                var attempt = Attempts.FirstOrDefault(a => a.AttemptId == parsed.AttemptId) ??
                              Attempts.FirstOrDefault(a => String.IsNullOrWhiteSpace(a.AttemptId));
                if(attempt == null)
                    throw new OutOfMemoryException("No more application attempts available! Try to initialize more attempts.");

                if(IsSelfMonitoring)
                {
                    attempt.AttemptId = parsed.AttemptId;
                }
                else
                {
                    attempt.SetStatus(parsed);
                    attempt.IsSelfMonitoring = IsSelfMonitoring;
                    attempt.MonitorStatus();
                }
            }
        }

        /// <summary>
        /// Sets the status based on the given <see cref="IParsedComponent"/>
        /// </summary>
        /// <param name="parsed">The parsed component</param>
        public void SetStatus(IParsedComponent parsed)
        {
            var app = parsed as IApplicationResult;
            AppId = app.AppId;
            Name = app.AppName;
            StartTime = app.StartTime;
            EndTime = app.FinishTime;
            Progress = app.Progess;
            State = app.State;
            FinalStatus = app.FinalStatus;
            AmHost = app.AmHost;
            AllocatedMb = app.AllocatedMb;
            AllocatedVcores = app.AllocatedVcores;
            MbSeconds = app.MbSeconds;
            VcoreSeconds = app.VcoreSeconds;
            PreemptedMb = app.PreemptedMb;
            PreemptedVcores = app.PreemptedVcores;
            AmContainerPreempted = app.AmContainerPreempted;
            NonAmContainerPreempted = app.NonAmContainerPreempted;
            CurrentRunningContainers = app.RunningContainers;
            Diagnostics = app.Diagnostics;
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
            if(IsSelfMonitoring && !String.IsNullOrWhiteSpace(AppId))
                MonitorStatus();
        }

        /// <summary>
        /// Stops/kills the app execution if possible and returns
        /// true if app was stopped already or was succesfully killed
        /// </summary>
        /// <returns>True if app is stopped/killed</returns>
        public bool StopApp()
        {
            if(!String.IsNullOrWhiteSpace(AppId)/* && State != EAppState.None && State != EAppState.NotStartedYet && IsKillable*/)
            {
                return FaultConnector.KillApplication(AppId);
            }
            return true;
        }

        #endregion

        #region Fault Effects

        /// <summary>
        /// Fault effect for <see cref="YarnApp.KillApp"/>
        /// </summary>
        [FaultEffect(Fault = nameof(KillApp))]
        public class KillAppEffect : YarnApp
        {
            public override void Update()
            {
                base.Update();
                StopApp();
            }
        }

        #endregion
    }
}