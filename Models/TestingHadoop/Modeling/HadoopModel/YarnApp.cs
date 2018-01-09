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
using System.Linq;
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
        /// The connector to use for node control
        /// </summary>
        public IHadoopConnector Connector { get; set; }

        /// <summary>
        /// <see cref="YarnAppAttempt"/> for this <see cref="YarnApp"/>
        /// </summary>
        public List<YarnAppAttempt> Attempts { get; } = new List<YarnAppAttempt>();

        /// <summary>
        /// Starting <see cref="Client"/> of this app
        /// </summary>
        public Client StartingClient { get; set; }

        /// <summary>
        /// Current state
        /// </summary>
        public EAppState State { get; set; } = EAppState.NOT_STARTED_YET;

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
        /// Ending Time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Main ApplicationMaster Host
        /// </summary>
        public YarnNode AmHost { get; set; }

        /// <summary>
        /// Allocated Memory MB-seconds
        /// </summary>
        public int AllocatedMemory { get; set; }

        /// <summary>
        /// Allocated CPU vcore-seconds
        /// </summary>
        public int AllocatedCpu { get; set; }

        /// <summary>
        /// Current Progress
        /// </summary>
        public int Progress { get; set; }

        #endregion

        #region IYarnReadable Methods

        /// <summary>
        /// Parser to read
        /// </summary>
        public IHadoopParser Parser { get; set; }

        /// <summary>
        /// Reads the current state from Hadoop
        /// </summary>
        public void GetStatus()
        {
            var parsed = Parser.ParseAppDetails(AppId);

            if(parsed != null)
            {
                Name = parsed.AppName;
                StartTime = parsed.StartTime;
                EndTime = parsed.FinishTime;
                Progress = parsed.Progess;
                State = parsed.State;
                AmHost = parsed.AmHost;
                AllocatedMemory = parsed.MbSeconds;
                AllocatedCpu = parsed.VcoreSeconds;

                var attempts = Parser.ParseAppAttemptList(AppId);
                if(attempts.Length > 0)
                {
                    foreach(var con in attempts)
                    {
                        if(Attempts.All(c => c.AttemptId != con.AttemptId))
                        {
                            var usingCont = Attempts.First(c => String.IsNullOrWhiteSpace(c.AttemptId));
                            if(usingCont == null)
                                throw new InsufficientMemoryException("No more application attempts available!" +
                                                                      " Try to initialize more container space.");
                            usingCont.AttemptId = con.AttemptId;
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        public override void Update()
        {
            if(!String.IsNullOrWhiteSpace(AppId))
                GetStatus();
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
                if(State != EAppState.FAILED && State != EAppState.FINISHED && State != EAppState.KILLED &&
                   State != EAppState.NOT_STARTED_YET)
                    Connector.KillApplication(AppId);
            }
        }

        #endregion
    }
}