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
using System.Linq;
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
        [NodeFault]
        public readonly Fault NodeConnectionErrorFault = new TransientFault();

        /// <summary>
        /// Fault for dead nodes
        /// </summary>
        [NodeFault]
        public readonly Fault NodeDeadFault = new TransientFault();

        /// <summary>
        /// Indicates if a fault is activated
        /// </summary>
        public virtual bool IsFaultActive => false;

        #endregion

        #region Properties

        /// <summary>
        /// Logger
        /// </summary>
        private static log4net.ILog Logger { get; } = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The connector to use for fault handling
        /// </summary>
        [NonSerializable]
        public IHadoopConnector FaultConnector => Model.UsingFaultingConnector;

        ///// <summary>
        ///// <see cref="YarnApp" />s executing by this node
        ///// </summary>
        //public List<YarnApp> ExecutingApps { get; private set; }

        ///// <summary>
        ///// Running Containers on this Node
        ///// </summary>
        //public List<YarnAppContainer> Containers { get; private set; }

        /// <summary>
        /// Node ID, based on its Name
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Connected <see cref="YarnController"/>
        /// </summary>
        public YarnController Controller { get; set; }

        /// <summary>
        /// Indicates if this <see cref="YarnNode"/> is aktive
        /// </summary>
        public virtual bool IsActive { get; set; }

        /// <summary>
        /// Indicates if this <see cref="YarnNode"/> connection is acitve
        /// </summary>
        public virtual bool IsConnected { get; set; }

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
        [NonSerializable]
        public long MemoryCapacity => MemoryUsed + MemoryAvailable;

        /// <summary>
        /// Memory usage in percentage
        /// </summary>
        [NonSerializable]
        public double MemoryUsage => MemoryCapacity > 0 ? (double)MemoryUsed / MemoryCapacity : 0.0;

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
        [NonSerializable]
        public long CpuCapacity => CpuUsed + CpuAvailable;

        /// <summary>
        /// CPU core usage in percentage
        /// </summary>
        [NonSerializable]
        public double CpuUsage => CpuCapacity > 0 ? (double)CpuUsed / CpuCapacity : 0.0;

        /// <summary>
        /// Number of current running containers
        /// </summary>
        public int RunningContainerCount { get; set; }

        /// <summary>
        /// Latest health update to <see cref="YarnController"/>
        /// </summary>
        public DateTime LastHealthUpdate { get; set; }

        /// <summary>
        /// Health report for the node
        /// </summary>
        // public string HealthReport { get; set; }
        public char[] HealthReportActual { get; } = new char[ModelSettings.DiagnosticsLength];

        /// <summary>
        /// Health report for the node as string, based on <see cref="HealthReportActual"/>
        /// </summary>
        [NonSerializable]
        public string HealthReport
        {
            get { return ModelUtilities.GetCharArrayAsString(HealthReportActual); }
            set { ModelUtilities.SetCharArrayOnString(HealthReportActual, value); }
        }

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
        /// <param name="controller">Connected <see cref="YarnController"/></param>
        public YarnNode(string name, YarnController controller)
            : base(name, 8042)
        {
            InitYarnNode();

            Controller = controller;
        }

        /// <summary>
        /// Initializes a new <see cref="YarnNode"/>
        /// </summary>
        /// <param name="name">Name of the Host</param>
        /// <param name="controller">Connected <see cref="YarnController"/></param>
        /// <param name="httpUrl">Http url of the node</param>
        public YarnNode(string name, YarnController controller, string httpUrl)
            : base(name, httpUrl)
        {
            InitYarnNode();

            Controller = controller;
        }

        private void InitYarnNode()
        {
            //ExecutingApps = new List<YarnApp>();
            //Containers = new List<YarnAppContainer>();
            IsActive = true;
            IsConnected = true;
            State = ENodeState.None;
            IsSelfMonitoring = false;
            NodeId = $"{Name}:45454";
        }

        #endregion

        #region IYarnReadable Methods

        /// <summary>
        /// Indicates if the data is collected and parsed by the component itself
        /// </summary>
        public bool IsSelfMonitoring { get; set; }

        /// <summary>
        /// S# constraints for the oracle based on requirement for the SuT
        /// </summary>
        [NonSerializable]
        public Func<bool>[] SutConstraints => new Func<bool>[]
        {
            // 4 defects are recognized
            () =>
            {
                OutputUtilities.PrintTestConstraint(4, GetId());
                if(IsActive && IsConnected && State == ENodeState.RUNNING) return true;
                if((!IsActive || !IsConnected) && State != ENodeState.RUNNING) return true;
                return false;
            }
        };

        /// <summary>
        /// Constraints to check the requirements of the test suite itself
        /// </summary>
        public Func<bool>[] TestConstraints => new Func<bool>[]
        {
            // 6 defect nodes are recognized
            // 7 faults are injected/repaired
            () =>
            {
                OutputUtilities.PrintTestConstraint(6, GetId());
                OutputUtilities.PrintTestConstraint(7, GetId());
                return SutConstraints[0]();
            },
        };

        /// <summary>
        /// Returns the ID of the component
        /// </summary>
        public string GetId()
        {
            return NodeId;
        }

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
            HealthReport = node.HealthReport;
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
            if(!IsFaultActive && !String.IsNullOrWhiteSpace(NodeId))
            {
                StartNode();
                StartConnection();
                MonitorStatus();
            }
            HandleFaultingOnSingleton();
        }

        /// <summary>
        /// Handles the saving of <see cref="IsActive"/> and <see cref="IsConnected"/>
        /// in the node that is saved by <see cref="Model.Instance"/> and not in
        /// the simulator copy.
        /// </summary>
        private void HandleFaultingOnSingleton()
        {
            var nodeInSingleton = Model.Instance.Nodes.First(n => n.Name == Name);
            if(this != nodeInSingleton)
            {
                nodeInSingleton.IsActive = IsActive;
                nodeInSingleton.IsConnected = IsConnected;
            }
        }

        /// <summary>
        /// Starts the node connection if the node is active and returns
        /// if the node is active and the node connection is active
        /// </summary>
        /// <param name="retry">True for a retry on failing start node connection</param>
        /// <returns>True if node is active and node connection is active</returns>
        public bool StartConnection(bool retry = true)
        {
            if(Name.EndsWith("-0"))
                return true;

            if(IsActive && !IsConnected)
            {
                Logger.Info($"Start connection on node {Name}");
                var isStarted = FaultConnector.StartNodeNetConnection(Name);
                if(isStarted)
                    IsConnected = true;
                else if(retry)
                    StartConnection(false);
            }
            return IsActive && IsConnected;
        }

        /// <summary>
        /// Stops the node connection if the node is active and returns
        /// if the node is active and the node connection is inactive
        /// </summary>
        /// <param name="retry">True for a retry on failing stop node connection</param>
        /// <returns>True if node is active and node connection is inactive</returns>
        public bool StopConnection(bool retry = true)
        {
            if(Name.EndsWith("-0"))
                return true;

            if(IsActive && IsConnected)
            {
                Logger.Info($"Stop connection on node {Name}");
                var isStopped = FaultConnector.StopNodeNetConnection(Name);
                if(isStopped)
                    IsConnected = false;
                else if(retry)
                    StopConnection(false);
            }
            return IsActive && !IsConnected;
        }

        /// <summary>
        /// Starts the node and returns if the node is active
        /// </summary>
        /// <param name="retry">True for a retry on failing start node</param>
        /// <returns>True if node is active</returns>
        public bool StartNode(bool retry = true)
        {
            if(Name.EndsWith("-0"))
                return true;

            if(!IsActive)
            {
                Logger.Info($"Start node {Name}");
                var isStarted = FaultConnector.StartNode(Name);
                if(isStarted)
                    IsActive = true;
                else if(retry)
                    StartNode(false);
            }
            return IsActive;
        }

        /// <summary>
        /// Stops the node and returns if the node is inactive
        /// </summary>
        /// <param name="retry">True for a retry on failing stop node</param>
        /// <returns>True if node is inactive</returns>
        public bool StopNode(bool retry = true)
        {
            if(Name.EndsWith("-0"))
                return true;

            if(IsActive)
            {
                Logger.Info($"Stop node {Name}");
                var isStopped = FaultConnector.StopNode(Name);
                if(isStopped)
                    IsActive = false;
                else if(retry)
                    StopNode(false);
            }
            return !IsActive;
        }

        #endregion

        #region Fault Effects

        /// <summary>
        /// Fault effect for <see cref="YarnNode.NodeConnectionErrorFault"/>
        /// </summary>
        [FaultEffect(Fault = nameof(NodeConnectionErrorFault))]
        [Priority(0)] // will be ignored if NodeDeadFault is active
        public class NodeConnectionErrorEffect : YarnNode
        {
            public override bool IsFaultActive => true;

            public override void Update()
            {
                StopConnection();
                base.Update();
            }
        }

        /// <summary>
        /// Fault effect for <see cref="YarnNode.NodeDeadFault"/>
        /// </summary>
        [FaultEffect(Fault = nameof(NodeDeadFault))]
        [Priority(1)]
        public class NodeDeadEffect : YarnNode
        {
            public override bool IsFaultActive => true;

            public override void Update()
            {
                StopNode();
                base.Update();
            }
        }

        #endregion
    }
}