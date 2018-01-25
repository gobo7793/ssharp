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

using System.Collections.Generic;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling
{
    /// <summary>
    /// Hadoop model base class
    /// </summary>
    public class Model : ModelBase
    {
        #region General settings

        //public const int MaxApplicationCount = 0xFF;
        //public const int MaxAppAttemptCount = 0xF;
        //public const int MaxContainerCount = 0xF;

        public const string NodeNamePrefix = "compute-";

        #endregion

        #region Model components

        public string Name { get; }

        public Client Client { get; set; }

        [Root(RootKind.Controller)]
        public YarnController Controller { get; set; }

        [Root(RootKind.Plant)]
        public Dictionary<string, YarnNode> Nodes { get; } = new Dictionary<string, YarnNode>();

        [Root(RootKind.Plant)]
        public List<YarnApp> Applications { get; } = new List<YarnApp>();

        [Root(RootKind.Plant)]
        public List<YarnAppAttempt> AppAttempts { get; } = new List<YarnAppAttempt>();

        [Root(RootKind.Plant)]
        public List<YarnAppContainer> AppContainers { get; } = new List<YarnAppContainer>();

        #endregion

        #region Base methods

        /// <summary>
        /// Initializes a new model
        /// </summary>
        /// <param name="name">Model name</param>
        public Model(string name = "")
        {
            Name = name;
        }

        #endregion

        #region Configurations

        /// <summary>
        /// Initializes the default Config
        /// </summary>
        public void InitializeDefaultInstance()
        {
            InitConfig1();
        }

        /// <summary>
        /// Initialize configuration for model testing
        /// </summary>
        /// <param name="parser">The parser to use</param>
        /// <param name="connector">The connector to use</param>
        public void InitTestConfig(IHadoopParser parser, IHadoopConnector connector)
        {
            InitBaseComponents();
            InitYarnNodes(4, parser, connector);

            InitApplications(4, parser, connector);
            InitAppAttempts(4, parser);
            InitContainers(16, parser);
        }


        /// <summary>
        /// Initizalizes the config for only one pi calculation
        /// </summary>
        public void InitConfig1()
        {
            IHadoopConnector connector = null;
            var parser = new CmdLineParser(this, connector);

            InitBaseComponents();
            InitYarnNodes(4, parser, connector);

            InitApplications(1, parser, connector);
            InitAppAttempts(1, parser);
            InitContainers(16, parser);
        }

        #endregion

        #region Component Inits

        /// <summary>
        /// Init base components like client and hadoop controller
        /// </summary>
        private void InitBaseComponents()
        {
            Controller = new YarnController();
            Client = new Client();

            Controller.ConnectedClient = Client;
            Client.ConnectedYarnController = Controller;
        }

        /// <summary>
        /// Init yarn compute nodes
        /// </summary>
        /// <param name="nodeCount">Instances count</param>
        /// <param name="parser">Hadoop parser to use</param>
        /// <param name="connector">Hadoop connector to use for faults</param>
        private void InitYarnNodes(int nodeCount, IHadoopParser parser, IHadoopConnector connector)
        {
            for(int i = 1; i <= nodeCount; i++)
            {
                var node = new YarnNode
                {
                    Name = NodeNamePrefix + i,
                    Controller = Controller,
                    Parser = parser,
                    Connector = connector,
                };

                Controller.ConnectedNodes.Add(node);
                Nodes[node.Name] = node;
            }
        }

        /// <summary>
        /// Init application instances
        /// </summary>
        /// <param name="appCount">Instances count</param>
        /// <param name="parser">Hadoop-Parser to use</param>
        /// <param name="connector">Hadoop connector to use for faults</param>
        private void InitApplications(int appCount, IHadoopParser parser, IHadoopConnector connector)
        {
            for(int i = 0; i < appCount; i++)
            {
                var app = new YarnApp
                {
                    StartingClient = Client,
                    Parser = parser,
                    Connector = connector,
                };

                Client.StartingYarnApps.Add(app);
                Applications.Add(app);
            }
        }

        /// <summary>
        /// Init application attempt instances
        /// </summary>
        /// <param name="attemptCount">Attempt instances count per app</param>
        /// <param name="parser">Hadoop-Parser to use</param>
        private void InitAppAttempts(int attemptCount, IHadoopParser parser)
        {
            foreach(var app in Applications)
            {
                for(int i = 0; i < attemptCount; i++)
                {
                    var attempt = new YarnAppAttempt
                    {
                        App = app,
                        Parser = parser,
                    };

                    app.Attempts.Add(attempt);
                    AppAttempts.Add(attempt);
                }
            }
        }

        /// <summary>
        /// Init application container instances
        /// </summary>
        /// <param name="containerCount">Container instances count per attempt</param>
        /// <param name="parser">Hadoop-Parser to use</param>
        private void InitContainers(int containerCount, IHadoopParser parser)
        {
            foreach(var attempt in AppAttempts)
            {
                for(int i = 0; i < containerCount; i++)
                {
                    var container = new YarnAppContainer
                    {
                        AppAttempt = attempt,
                        Parser = parser,
                    };

                    attempt.Containers.Add(container);
                    AppContainers.Add(container);
                }
            }
        }

        #endregion

    }
}