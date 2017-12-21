// The MIT License (MIT)
// 
// Copyright (c) 2014-2017, Institute for Software & Systems Engineering
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
using ISSE.SafetyChecking.Modeling;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling
{
    /// <summary>
    /// Basis class for yarn nodes
    /// </summary>
    public class YarnNode : Component
    {
        /// <summary>
        /// Fault for connection errors
        /// </summary>
        public readonly Fault NodeConnectionError = new TransientFault();

        /// <summary>
        /// Fault for dead nodes
        /// </summary>
        public readonly Fault NodeDead = new TransientFault();

        /// <summary>
        /// Name of the node
        /// </summary>
        public string Name
        {
            get => default(string);
            set
            {
            }
        }

        /// <summary>
        /// Indicates if node is aktive
        /// </summary>
        public bool IsActive
        {
            get => default(bool);
            set
            {
            }
        }

        /// <summary>
        /// Indicates if the node connection is acitve
        /// </summary>
        public bool IsConnected
        {
            get => default(bool);
            set
            {
            }
        }

        /// <summary>
        ///   <see cref="YarnApp" />s executing by this node
        /// </summary>
        public List<YarnApp> ExecutingApps
        {
            get => default(List<YarnApp>);
            set
            {
            }
        }

        /// <summary>
        /// Gets the current node status
        /// </summary>
        public void GetStatus()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Fault effect for <see cref="NodeConnectionError"/>
        /// </summary>
        [FaultEffect(Fault = nameof(NodeConnectionError))]
        internal class NodeConnectionErrorFault : YarnNode
        {

        }

        /// <summary>
        /// Fault effect for <see cref="NodeDead"/>
        /// </summary>
        [FaultEffect(Fault = nameof(NodeDead))]
        internal class NodeDeadFault : YarnNode
        {

        }
    }
}