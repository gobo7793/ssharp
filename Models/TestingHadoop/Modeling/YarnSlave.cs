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

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling
{
    /// <summary>
    /// YARN slave node which executes <see cref="YarnApp"/>s
    /// </summary>
    public class YarnSlave : YarnNode, IYarnReadable
    {

        /// <summary>
        /// Connected <see cref="YarnMaster"/>
        /// </summary>
        public YarnMaster Controller { get; set; }

        /// <summary>
        /// <see cref="YarnApp" />s executing by this node
        /// </summary>
        public List<YarnApp> ExecutingApps { get; set; }

        /// <summary>
        /// Indicates if this <see cref="YarnSlave"/> is aktive
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates if this <see cref="YarnSlave"/> connection is acitve
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Node ID
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Currenet State
        /// </summary>
        public string NodeState { get; set; }

        /// <summary>
        /// Running Containers on this Node
        /// </summary>
        public List<YarnAppContainer> Containers { get; set; }

        /// <summary>
        /// Current Memory in use in MB
        /// </summary>
        public int MemoryUsed { get; set; }

        /// <summary>
        /// Total Memory available in MB
        /// </summary>
        public int MemoryCapacity { get; set; }

        /// <summary>
        /// Current CPU vcores in use
        /// </summary>
        public int CpuUsed { get; set; }

        /// <summary>
        /// Total CPU vcores available
        /// </summary>
        public int CpuCapacity { get; set; }

        /// <summary>
        /// Initializes a new <see cref="YarnSlave"/>
        /// </summary>
        public YarnSlave()
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