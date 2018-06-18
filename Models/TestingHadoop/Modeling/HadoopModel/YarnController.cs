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
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// YARN Controller
    /// </summary>
    public class YarnController : YarnHost
    {

        #region Properties

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

        /// <summary>
        /// The monitored MARP values, set null to disable MARP monitoring
        /// </summary>
        public static List<double> MarpValues { get; private set; }

        /// <summary>
        /// Parser to monitor MARP value
        /// </summary>
        [NonSerializable]
        public IHadoopParser MarpParser => Model.Instance.UsingMarpParser;

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
        public YarnController(string name)
            : this(name, 0)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="YarnController"/>
        /// </summary>
        /// <param name="name">Name of the Host</param>
        /// <param name="marpValuesCount">The maximum saveable marp value count</param>
        public YarnController(string name, int marpValuesCount) : base(name, 8088)
        {
            if(marpValuesCount > 0)
            {
                var monitoringCount = marpValuesCount * 2;
                MarpValues = new List<double>(monitoringCount+1);
                for(int i = 0; i <= monitoringCount; i++)
                    MarpValues.Add(-1);
            }
            else
            {
                MarpValues = null;
            }
        }

        #endregion

        #region General methods

        public override void Update()
        {
            // pre monitoring for getting more values
            MonitorMarp();

            // only in this model in this place to be sure that constraint
            // checking will be done after updating and monitoring benchmarks
            foreach(var client in ConnectedClients)
                client.UpdateBenchmark();

            // optional, to allocate at least the AM container
            ModelUtilities.Sleep(5);

            MonitorAll();

            Logger.Info("Checking SuT constraints.");
            Oracle.ValidateConstraints(EConstraintType.Sut);
            Oracle.IsReconfPossible();

            Logger.Info("Checking test constraints");
            Oracle.ValidateConstraints(EConstraintType.Test);
        }

        #endregion

        #region Monitoring methods

        /// <summary>
        /// Monitors all nodes, apps and marp value
        /// </summary>
        public void MonitorAll()
        {
            MonitorNodes();
            MonitorApps();
            MonitorMarp();
        }

        /// <summary>
        /// Monitors all node informations
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
        /// Monitors all apps executed on the cluster and their informations
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

        /// <summary>
        /// Monitors the current MARP value
        /// </summary>
        public void MonitorMarp()
        {
            if(MarpValues == null)
                return;

            Logger.Debug("Monitoring MARP value");
            var marpValue = MarpParser.ParseMarpValue();
            var index = MarpValues.IndexOf(-1);
            if(index < 0)
                throw new OutOfMemoryException($"Failed to save MARP value {marpValue}: No more memory available.");
            MarpValues[index] = marpValue;
        }

        #endregion

        #region Constraints

        /// <summary>
        /// Constraints to check the requirements of the test suite itself
        /// </summary>
        [Hidden(HideElements = true)]
        public Func<bool>[] TestConstraints => new Func<bool>[]
        {
            // marp value is changing
            () =>
            {
                OutputUtilities.PrintTestConstraint("marp value is changing", "controller");
                var usableValues = MarpValues.Where(d => d >= 0);
                if(!usableValues.Any())
                    return true; // not enough values to compare
                var uniqueMarpValues = usableValues.Distinct();
                return uniqueMarpValues.Count() > 1;
            },
            // 8 if no node is running no reconfiguration possibility is recognized
            () =>
            {
                OutputUtilities.PrintTestConstraint(
                    "no node is running no reconfiguration possibility is recognized", "controller");
                var isOneNodeAlive = ConnectedNodes.Any(n => n.State == ENodeState.RUNNING);
                return isOneNodeAlive;
            },
            // 10 multihost cluster is working
            () =>
            {
                OutputUtilities.PrintTestConstraint("multihost cluster is working", "controller");
                if(ModelSettings.HostsCount <= 1)
                    return true;
                var nodeCount = ConnectedNodes.Count(n => n.State != ENodeState.None);
                return nodeCount == ModelUtilities.GetFullNodeCount();
            },
            // 11 multiple apps can be running on same time
            () =>
            {
                OutputUtilities.PrintTestConstraint("multiple apps can be running on same time", "controller");
                if(ConnectedClients.Count <= 1)
                    return true;
                return ConnectedClients.All(c => c.CurrentExecutingApp.FinalStatus != EFinalStatus.None);
            }
        };

        #endregion

    }
}