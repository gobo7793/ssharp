﻿// The MIT License (MIT)
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
using System.Reflection;
using ISSE.SafetyChecking.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// YARN slave node which executes <see cref="YarnApp"/>s
    /// </summary>
    public class YarnNode : YarnHost, IYarnReadable
    {
        #region Faults

        /// <summary>
        /// Fault for connection errors
        /// </summary>
        public readonly Fault NodeConnectionError = new TransientFault();

        /// <summary>
        /// Fault for dead nodes
        /// </summary>
        public readonly Fault NodeDead = new TransientFault();

        #endregion

        #region Properties

        private string _NodeId;

        protected override string HttpPort => "8042";

        /// <summary>
        /// The connector to use for fault handling
        /// </summary>
        public IHadoopConnector FaultConnector { get; set; }

        /// <summary>
        /// <see cref="YarnApp" />s executing by this node
        /// </summary>
        public List<YarnApp> ExecutingApps { get; private set; }

        /// <summary>
        /// Running Containers on this Node
        /// </summary>
        public List<YarnAppContainer> Containers { get; private set; }

        /// <summary>
        /// Node ID, based on its Name
        /// </summary>
        public string NodeId
        {
            get
            {
                if(String.IsNullOrWhiteSpace(_NodeId))
                    _NodeId = Name + ":45454";
                return _NodeId;
            }
        }

        /// <summary>
        /// Connected <see cref="YarnController"/>
        /// </summary>
        public YarnController Controller { get; set; }

        /// <summary>
        /// Indicates if this <see cref="YarnNode"/> is aktive
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates if this <see cref="YarnNode"/> connection is acitve
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Current State
        /// </summary>
        public ENodeState State { get; set; }

        /// <summary>
        /// Current Memory in use in MB
        /// </summary>
        public long MemoryUsed { get; set; }

        /// <summary>
        /// Current Memory available in MB
        /// </summary>
        public long MemoryAvailable { get; set; }

        /// <summary>
        /// Total Memory capacity in MB
        /// </summary>
        public long MemoryCapacity => MemoryUsed + MemoryAvailable;

        /// <summary>
        /// Current CPU vcores in use
        /// </summary>
        public long CpuUsed { get; set; }

        /// <summary>
        /// Current CPU vcores available
        /// </summary>
        public long CpuAvailable { get; set; }

        /// <summary>
        /// Total CPU vcores capacity
        /// </summary>
        public long CpuCapacity => CpuUsed + CpuAvailable;

        /// <summary>
        /// Number of current running containers
        /// </summary>
        public int RunningContainerCount { get; set; }

        /// <summary>
        /// Latest health update to <see cref="YarnController"/>
        /// </summary>
        public DateTime LastHealthUpdate { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new empty <see cref="YarnNode"/>
        /// </summary>
        public YarnNode()
        {
            InitYarnNode();
        }

        /// <summary>
        /// Initializes a new <see cref="YarnNode"/>
        /// </summary>
        /// <param name="name">Name of the Host</param>
        /// <param name="parser">Parser to monitoring data from cluster</param>
        /// <param name="faultConnector">The connector to use for fault handling</param>
        /// <param name="controller">Connected <see cref="YarnController"/></param>
        public YarnNode(string name, IHadoopParser parser, IHadoopConnector faultConnector, YarnController controller)
            : base(name, parser)
        {
            InitYarnNode();

            FaultConnector = faultConnector;
            Controller = controller;
        }

        private void InitYarnNode()
        {
            ExecutingApps = new List<YarnApp>();
            Containers = new List<YarnAppContainer>();
            IsActive = true;
            IsConnected = true;
            State = ENodeState.None;
            IsSelfMonitoring = true;
        }

        #endregion

        #region IYarnReadable Methods

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

            var parsed = Parser.ParseNodeDetails(NodeId);
            if(parsed != null)
                SetStatus(parsed);
        }

        /// <summary>
        /// Sets the status based on the given <see cref="IParsedComponent"/>
        /// </summary>
        /// <param name="parsed">The parsed component</param>
        public void SetStatus(IParsedComponent parsed)
        {
            var node = parsed as INodeResult;
            State = node.NodeState;
            RunningContainerCount = node.RunningContainerCount;
            MemoryUsed = node.MemoryUsed;
            MemoryAvailable = node.MemoryAvailable;
            CpuUsed = node.CpuUsed;
            CpuAvailable = node.CpuAvailable;
            LastHealthUpdate = node.LastHealthUpdate;
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
            if(!String.IsNullOrWhiteSpace(NodeId))
            {
                IsActive = FaultConnector.StartNode(Name);
                IsConnected = FaultConnector.StartNodeNetConnection(Name);
                MonitorStatus();
            }
        }

        #endregion

        #region Fault Effects

        /// <summary>
        /// Fault effect for <see cref="YarnNode.NodeConnectionError"/>
        /// </summary>
        [FaultEffect(Fault = nameof(NodeConnectionError))]
        [Priority(0)] // will be ignored if NodeDead is active
        public class NodeConnectionErrorEffect : YarnNode
        {
            public override void Update()
            {
                if(IsActive)
                    IsConnected = !FaultConnector.StopNodeNetConnection(Name);
            }
        }

        /// <summary>
        /// Fault effect for <see cref="YarnNode.NodeDead"/>
        /// </summary>
        [FaultEffect(Fault = nameof(NodeDead))]
        [Priority(1)]
        public class NodeDeadEffect : YarnNode
        {
            public override void Update()
            {
                IsActive = !FaultConnector.StopNode(Name);
            }
        }

        #endregion
    }
}