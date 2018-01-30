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
        public List<YarnAppAttempt> Attempts { get; } = new List<YarnAppAttempt>();

        /// <summary>
        /// Starting <see cref="Client"/> of this app
        /// </summary>
        public Client StartingClient { get; set; }

        /// <summary>
        /// The command name to start the app
        /// </summary>
        public string StartName { get; set; }

        /// <summary>
        /// The command arguments to start the app
        /// </summary>
        public string StartArgs { get; set; }

        /// <summary>
        /// Current state
        /// </summary>
        public EAppState State { get; set; } = EAppState.NotStartedYet;

        /// <summary>
        /// Final status
        /// </summary>
        public EFinalStatus FinalStatus { get; set; } = EFinalStatus.None;

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

        #region IYarnReadable Methods

        /// <summary>
        /// Parser to read
        /// </summary>
        public IHadoopParser Parser { get; set; }

        /// <summary>
        /// Reads the current state from Hadoop
        /// </summary>
        public void ReadStatus()
        {
            var parsed = Parser.ParseAppDetails(AppId);

            if(parsed != null)
            {
                Name = parsed.AppName;
                StartTime = parsed.StartTime;
                EndTime = parsed.FinishTime;
                Progress = parsed.Progess;
                State = parsed.State;
                FinalStatus = parsed.FinalStatus;
                AmHost = parsed.AmHost;
                AllocatedMb = parsed.AllocatedMb;
                AllocatedVcores = parsed.AllocatedVcores;
                MbSeconds = parsed.MbSeconds;
                VcoreSeconds = parsed.VcoreSeconds;
                PreemptedMb = parsed.PreemptedMb;
                PreemptedVcores = parsed.PreemptedVcores;
                AmContainerPreempted = parsed.AmContainerPreempted;
                NonAmContainerPreempted = parsed.NonAmContainerPreempted;
                CurrentRunningContainers = parsed.RunningContainers;
                Diagnostics = parsed.Diagnostics;

                var attempts = Parser.ParseAppAttemptList(AppId);
                if(attempts.Length > 0)
                {
                    foreach(var con in attempts)
                    {
                        if(Attempts.All(c => c.AttemptId != con.AttemptId))
                        {
                            var usingAttempt = Attempts.Find(c => String.IsNullOrWhiteSpace(c.AttemptId));
                            if(usingAttempt == null)
                                throw new OutOfMemoryException("No more application attempts available!" +
                                                               " Try to initialize more attempt space.");
                            usingAttempt.AttemptId = con.AttemptId;
                        }
                    }
                }
            }
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
            if(!String.IsNullOrWhiteSpace(AppId))
                ReadStatus();
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
                if(IsKillable)
                    FaultConnector.KillApplication(AppId);
            }
        }

        #endregion
    }
}