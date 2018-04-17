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
using System.Collections.Generic;
using System.Linq;
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
        /// HTTP URL of the timeline server
        /// </summary>
        public string TimelineHttpUrl { get; }

        /// <summary>
        /// Connected <see cref="Client" />
        /// </summary>
        //public List<Client> ConnectedClients { get; private set; }
        [NonSerializable]
        public List<Client> ConnectedClients => Model.Instance.Clients;

        /// <summary>
        /// Connected <see cref="YarnNode" />s
        /// </summary>
        //public List<YarnNode> ConnectedNodes { get; private set; }
        [NonSerializable]
        public List<YarnNode> ConnectedNodes => Model.Instance.Nodes;

        /// <summary>
        /// The executed <see cref="YarnApp"/>s on the cluster
        /// </summary>
        //public List<YarnApp> Apps { get; private set; }
        [NonSerializable]
        public List<YarnApp> Apps => Model.Instance.Applications;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new empty <see cref="YarnController"/>
        /// </summary>
        public YarnController()
        {
            InitYarnController();
        }

        /// <summary>
        /// Initialize a new <see cref="YarnController"/>
        /// </summary>
        /// <param name="name">Name of the Host</param>
        public YarnController(string name) : base(name, 8088)
        {
            InitYarnController();
            TimelineHttpUrl = $"http://{Name}:8188";
        }

        private void InitYarnController()
        {
            //ConnectedClients = new List<Client>();
            //ConnectedNodes = new List<YarnNode>();
            //Apps = new List<YarnApp>();
        }

        #endregion

        #region General methods

        public override void Update()
        {
            // only in this model in this place to be sure that constraint
            // checking will be done after updating and monitoring benchmarks
            foreach(var client in ConnectedClients)
                client.UpdateBenchmark();

            ModelUtilities.Sleep(); // optional, to allocate at least the AM container

            MonitorNodes();
            MonitorApps();

            CheckConstraints();
            IsReconfPossible();
        }

        #endregion

        #region Monitoring methods

        /// <summary>
        /// Gets all node informations
        /// </summary>
        public void MonitorNodes()
        {
            var parsedNodes = Parser.ParseNodeList();
            foreach(var parsed in parsedNodes)
            {
                var node = ConnectedNodes.FirstOrDefault(n => n.NodeId == parsed.NodeId);
                if(node == null)
                    continue;

                node.SetStatus(parsed);
                node.IsSelfMonitoring = false;
            }
        }

        /// <summary>
        /// Gets all apps executed on the cluster and their informations
        /// </summary>
        public void MonitorApps()
        {
            var parsedApps = Parser.ParseAppList(EAppState.ALL);
            foreach(var parsed in parsedApps)
            {
                var app = Apps.FirstOrDefault(a => a.AppId == parsed.AppId);// ??
                //          Apps.FirstOrDefault(a => String.IsNullOrWhiteSpace(a.AppId));
                //if(app == null)
                //    throw new OutOfMemoryException("No more applications available! Try to initialize more applications.");

                if(app == null)
                    continue;

                app.SetStatus(parsed);
                app.IsSelfMonitoring = false;
                app.MonitorStatus();
            }
        }

        #endregion

        #region Constraints

        /// <summary>
        /// Checks the constraints for all YARN components
        /// </summary>
        public void CheckConstraints()
        {
            bool isComponentValid;
            foreach(var node in ConnectedNodes)
            {
                isComponentValid = ValidateConstraints(node);
                if(!isComponentValid)
                    return;
            }

            foreach(var app in Apps)
            {
                isComponentValid = ValidateConstraints(app);
                if(!isComponentValid)
                    return;

                foreach(var attempt in app.Attempts)
                {

                    isComponentValid = ValidateConstraints(attempt);
                    if(!isComponentValid)
                        return;

                    foreach(var container in attempt.Containers)
                    {

                        isComponentValid = ValidateConstraints(container);
                        if(!isComponentValid)
                            return;
                    }
                }
            }
        }

        /// <summary>
        /// Validate the constraints for the given yarn component
        /// </summary>
        /// <param name="yarnComponent">The yarn component to validate</param>
        /// <returns>True if constraints are valid</returns>
        internal bool ValidateConstraints(IYarnReadable yarnComponent)
        {
            var isComponentValid = yarnComponent.Constraints.All(constraint => constraint());
            if(!isComponentValid)
                Logger.Warning($"YARN component not valid: {yarnComponent.GetId()}");
            return isComponentValid;
        }

        #endregion

        #region Reconfiguration

        /// <summary>
        /// Validates if reconfiguration for the cluster would be possible
        /// </summary>
        /// <returns>True if reconfiguration is possible</returns>
        public bool IsReconfPossible()
        {
            var isReconfPossible = ConnectedNodes.Any(n => n.State == ENodeState.RUNNING);
            if(!isReconfPossible)
                Logger.Exception("No reconfiguration possible!");
            return isReconfPossible;
        }

        #endregion

    }
}