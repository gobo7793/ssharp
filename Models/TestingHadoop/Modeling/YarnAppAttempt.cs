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
using System.Linq;
using System.Text;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling
{
    /// <summary>
    /// Execution attempt of a <see cref="YarnApp"/>
    /// </summary>
    public class YarnAppAttempt : Component, IYarnReadable
    {
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
        public AppState State { get; set; }

        /// <summary>
        /// Running Containers
        /// </summary>
        public List<YarnAppContainer> Containers { get; }

        /// <summary>
        /// Container for ApplicationMaster
        /// </summary>
        public YarnAppContainer AmContainer { get; set; }

        /// <summary>
        /// <see cref="YarnNode"/> the ApplicationMaster is running
        /// </summary>
        public YarnNode AmHost { get; set; }

        /// <summary>
        /// Initializes a new <see cref="YarnAppAttempt"/>
        /// </summary>
        public YarnAppAttempt()
        {
            Containers = new List<YarnAppContainer>();
        }

        /// <summary>
        /// Reads the current state from Hadoop
        /// </summary>
        public void GetStatus()
        {
            throw new NotImplementedException();
        }
    }
}