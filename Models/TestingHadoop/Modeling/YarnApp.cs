﻿// The MIT License (MIT)
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
    /// Application/Job to run on the Hadoop cluster
    /// </summary>
    public class YarnApp : Component
    {
        /// <summary>
        /// App will be killed
        /// </summary>
        public readonly Fault KillApp = new PermanentFault();

        /// <summary>
        /// Starting <see cref="Client"/> of this app
        /// </summary>
        public Client StartingClient
        {
            get => default(Client);
            set
            {
            }
        }

        /// <summary>
        /// Running <see cref="YarnSlave"/> for this app
        /// </summary>
        public List<YarnNode> ExecutingNodes
        {
            get => default(List<YarnNode>);
            set
            {
            }
        }

        /// <summary>
        /// Current state
        /// </summary>
        public AppState AppState
        {
            get => default(AppState);
            set
            {
            }
        }

        /// <summary>
        /// Name of the app
        /// </summary>
        public string Name
        {
            get => default(string);
            set
            {
            }
        }

        /// <summary>
        /// Containers from this app
        /// </summary>
        public List<YarnAppContainer> Containers
        {
            get => default(List<YarnAppContainer>);
            set
            {
            }
        }

        /// <summary>
        /// ID of the app
        /// </summary>
        public string AppId
        {
            get => default(string);
            set
            {
            }
        }

        /// <summary>
        /// Fault effect for <see cref="KillApp"/>
        /// </summary>
        [FaultEffect(Fault = nameof(KillApp))]
        internal class KillAppEffect : YarnNode
        {

        }
    }
}