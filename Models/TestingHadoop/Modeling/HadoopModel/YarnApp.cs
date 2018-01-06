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
using ISSE.SafetyChecking.Modeling;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// Application/Job to run on the Hadoop cluster
    /// </summary>
    public class YarnApp : Component, IYarnReadable
    {
        /// <summary>
        /// App will be killed
        /// </summary>
        public readonly Fault KillApp = new PermanentFault();

        /// <summary>
        /// Starting <see cref="Client"/> of this app
        /// </summary>
        public Client StartingClient { get; set; }

        /// <summary>
        /// Current state
        /// </summary>
        public AppState State { get; set; }

        /// <summary>
        /// Name of the app
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// <see cref="YarnAppAttempt"/> for this <see cref="YarnApp"/>
        /// </summary>
        public List<YarnAppAttempt> AppAttempts { get; }

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
        public int Progress
        {
            get => default(int);
            set
            {
            }
        }

        /// <summary>
        /// Initializes a new <see cref="YarnApp"/>
        /// </summary>
        public YarnApp()
        {
            AppAttempts = new List<YarnAppAttempt>();
        }

        /// <summary>
        /// Reads the current state from Hadoop
        /// </summary>
        public void GetStatus()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fault effect for <see cref="KillApp"/>
        /// </summary>
        [FaultEffect(Fault = nameof(KillApp))]
        public class KillAppEffect : YarnHost
        {

        }
    }
}