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
        public List<YarnAppContainer> Containers { get; } = new List<YarnAppContainer>();

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
        public EAppState State { get; set; } = EAppState.NotStartedYet;

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
            var parsed = Parser.ParseAppAttemptDetails(AttemptId);

            if(parsed != null)
            {
                State = parsed.State;
                AmContainerId = parsed.AmContainerId;
                AmHost = parsed.AmHost;
                TrackingUrl = parsed.TrackingUrl;
                StartTime = parsed.StartTime;
                Diagnostics = parsed.Diagnostics;

                var containers = Parser.ParseContainerList(AttemptId);
                if(containers.Length > 0)
                {
                    foreach(var con in containers)
                    {
                        if(Containers.All(c => c.ContainerId != con.ContainerId))
                        {
                            var usingCont = Containers.First(c => String.IsNullOrWhiteSpace(c.ContainerId));
                            if(usingCont == null)
                                throw new InsufficientMemoryException("No more application attempt containers available!" +
                                                                      " Try to initialize more container space.");
                            usingCont.ContainerId = con.ContainerId;
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        public override void Update()
        {
            if(!String.IsNullOrWhiteSpace(AttemptId))
                GetStatus();
        }

        #endregion
    }
}