#region License
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
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using SafetySharp.CompilerServices;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// YARN Controller
    /// </summary>
    public class YarnController : YarnHost
    {

        #region Properties

        /// <summary>
        /// Indicates if the reconfiguration would be possible
        /// </summary>
        [NonSerializable]
        private bool _IsReconfPossible { get; set; }

        /// <summary>
        /// Indicates if all SuT constraints are valid
        /// </summary>
        [NonSerializable]
        private bool _IsSutConsraintsValid { get; set; }

        [NonSerializable]
        private static log4net.ILog Logger { get; } =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// HTTP URL of the timeline server
        /// </summary>
        [NonSerializable]
        public string TimelineHttpUrl => $"http://{Name}:8188";

        /// <summary>
        /// Connected <see cref="Client" />
        /// </summary>
        //public List<Client> ConnectedClients { get; private set; } = new List<Client>();
        [NonSerializable]
        public List<Client> ConnectedClients => Model.Instance.Clients;

        /// <summary>
        /// Connected <see cref="YarnNode" />s
        /// </summary>
        //public List<YarnNode> ConnectedNodes { get; private set; } = new List<YarnNode>();
        [NonSerializable]
        public List<YarnNode> ConnectedNodes => Model.Instance.Nodes;

        /// <summary>
        /// The executed <see cref="YarnApp"/>s on the cluster
        /// </summary>
        //public List<YarnApp> Apps { get; private set; } = new List<YarnApp>();
        [NonSerializable]
        public List<YarnApp> Apps => Model.Instance.Applications;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new empty <see cref="YarnController"/>
        /// </summary>
        public YarnController()
        {
        }

        /// <summary>
        /// Initialize a new <see cref="YarnController"/>
        /// </summary>
        /// <param name="name">Name of the Host</param>
        public YarnController(string name) : base(name, 8088)
        {
        }

        #endregion

        #region General methods

        public override void Update()
        {
            //// Logging, trace printing and timing here because the probabilistic simulator
            //// doesn't support single step execution
            //OutputUtilities.PrintStepStart();
            //var stepStartTime = DateTime.Now;

            // only in this model in this place to be sure that constraint
            // checking will be done after updating and monitoring benchmarks
            foreach(var client in ConnectedClients)
                client.UpdateBenchmark();

            // optional, to allocate at least the AM container
            ModelUtilities.Sleep(5);

            MonitorNodes();
            MonitorApps();

            _IsSutConsraintsValid = Oracle.ValidateConstraints(EConstraintType.Sut);
            _IsReconfPossible = Oracle.IsReconfPossible();

            Oracle.ValidateConstraints(EConstraintType.Test);

            //var stepTime = DateTime.Now - stepStartTime;
            //OutputUtilities.PrintDuration(stepTime);
            //if(stepTime < Model.MinStepTime)
            //    Thread.Sleep(Model.MinStepTime - stepTime);

            //OutputUtilities.PrintFullTrace(this);
        }

        #endregion

        #region Monitoring methods

        /// <summary>
        /// Gets all node informations
        /// </summary>
        public void MonitorNodes()
        {
            Logger.Debug("Monitoring nodes");

            var parsedNodes = Parser.ParseNodeList();
            foreach(var parsed in parsedNodes)
            {
                var node = ConnectedNodes.FirstOrDefault(n => n.NodeId == parsed.NodeId);
                if(node == null)
                    continue;

                node.IsSelfMonitoring = false;
                node.SetStatus(parsed);
            }
        }

        /// <summary>
        /// Gets all apps executed on the cluster and their informations
        /// </summary>
        public void MonitorApps()
        {
            Logger.Debug("Monitoring applications");

            var parsedApps = Parser.ParseAppList(EAppState.ALL);
            if(parsedApps == null)
                return;
            foreach(var parsed in parsedApps)
            {
                var app = Apps.FirstOrDefault(a => a.AppId == parsed.AppId);// ??
                //          Apps.FirstOrDefault(a => String.IsNullOrWhiteSpace(a.AppId));
                //if(app == null)
                //    throw new OutOfMemoryException("No more applications available! Try to initialize more applications.");

                if(app == null)
                    continue;

                app.IsSelfMonitoring = false;
                app.SetStatus(parsed);
                app.MonitorStatus();
            }
        }

        #endregion

        #region Constraints

        /// <summary>
        /// Constraints to check the requirements of the test suite itself
        /// </summary>
        [Hidden(HideElements = true)]
        public Func<bool>[] TestConstraints => new Func<bool>[]
        {
            // 8 if no node is running no reconfiguration possibility is recognized
            () =>
            {
                OutputUtilities.PrintTestConstraint(8, "controller");
                var isOneNodeAlive = ConnectedNodes.Any(n => n.State == ENodeState.RUNNING);
                return isOneNodeAlive == _IsReconfPossible;
            },
            // 10 multihost cluster is working
            () =>
            {
                OutputUtilities.PrintTestConstraint(10, "controller");
                if(ModelSettings.HostsCount <= 1)
                    return true;
                var nodeCount = ConnectedNodes.Count(n => n.State != ENodeState.None);
                return nodeCount == ModelUtilities.GetFullNodeCount();
            },
            // 11 multiple apps can be running on same time
            () =>
            {
                OutputUtilities.PrintTestConstraint(11, "controller");
                if(ConnectedClients.Count <= 1)
                return true;
                    return ConnectedClients.All(c => c.CurrentExecutingApp.FinalStatus != EFinalStatus.None);
            }
        };

        #endregion

    }
}