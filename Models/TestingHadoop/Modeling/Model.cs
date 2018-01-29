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

        /// <summary>
        /// Prefix for compute nodes
        /// </summary>
        public const string NodeNamePrefix = "compute-";

        /// <summary>
        /// Command (full path) for Hadoop setup script
        /// </summary>
        /// <remarks>
        /// Generic options for all commands can be inserted here.
        /// </remarks>
        public const string HadoopSetupScript = "/home/siegerge/hadoop-benchmark/setup-hadoop.sh -q";

        /// <summary>
        /// Hostname for the Hadoop cluster pc
        /// </summary>
        public const string SshHost = "swth11";

        /// <summary>
        /// Username for the Hadoop cluster pc
        /// </summary>
        public const string SshUsername = "siegerge";

        /// <summary>
        /// Full file path to the private key file to login
        /// </summary>
        public const string SshPrivateKeyFilePath = @"C:\Users\siegerge\sshnet";

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
        public void InitDefaultInstance()
        {
            InitConfig1();
        }

        /// <summary>
        /// Initialize configuration for model testing
        /// </summary>
        /// <param name="parser">The monitorParser to use</param>
        /// <param name="connector">The faultConnector to use</param>
        public void InitTestConfig(IHadoopParser parser, IHadoopConnector connector)
        {
            if(Controller == null)
                InitController();
            InitBaseComponents(connector);
            InitYarnNodes(4, parser, connector);

            InitApplications(4, parser, connector);
            InitAppAttempts(4, parser);
            InitContainers(32, parser);
        }


        /// <summary>
        /// Initizalizes the config for only one application
        /// </summary>
        public void InitConfig1()
        {
            InitController();

            var cmdConnector = new CmdConnector(SshHost, SshUsername, SshPrivateKeyFilePath, false, true, 1);
            var restConnector = new RestConnector(SshHost, SshUsername, SshPrivateKeyFilePath, Controller.HttpUrl, Controller.TimelineHttpUrl);
            var restParser = new JsonParser(this, restConnector);

            InitBaseComponents(cmdConnector);
            InitYarnNodes(4, restParser, cmdConnector);

            InitApplications(1, restParser, cmdConnector);
            InitAppAttempts(2, restParser);
            InitContainers(32, restParser);
        }

        #endregion

        #region Component Inits

        /// <summary>
        /// Init hadoop controller
        /// </summary>
        public void InitController()
        {
            Controller = new YarnController
            {
                Name = "controller",
            };
        }

        /// <summary>
        /// Init other base components like client
        /// </summary>
        /// <param name="submittingConnector">Connector to submitting <see cref="YarnApp"/></param>
        private void InitBaseComponents(IHadoopConnector submittingConnector)
        {
            Client = new Client
            {
                SubmittingConnector = submittingConnector,
            };

            Controller.ConnectedClient = Client;
            Client.ConnectedYarnController = Controller;
        }

        /// <summary>
        /// Init yarn compute nodes
        /// </summary>
        /// <param name="nodeCount">Instances count</param>
        /// <param name="monitorParser">Parser to use for monitoring</param>
        /// <param name="faultConnector">Connector to use for faults</param>
        private void InitYarnNodes(int nodeCount, IHadoopParser monitorParser, IHadoopConnector faultConnector)
        {
            for(int i = 1; i <= nodeCount; i++)
            {
                var node = new YarnNode
                {
                    Name = NodeNamePrefix + i,
                    Controller = Controller,
                    Parser = monitorParser,
                    FaultConnector = faultConnector,
                };

                Controller.ConnectedNodes.Add(node);
                Nodes[node.Name] = node;
            }
        }

        /// <summary>
        /// Init application instances
        /// </summary>
        /// <param name="appCount">Instances count</param>
        /// <param name="monitorParser">Parser to use for monitoring</param>
        /// <param name="faultConnector">Connector to use for faults</param>
        private void InitApplications(int appCount, IHadoopParser monitorParser, IHadoopConnector faultConnector)
        {
            for(int i = 0; i < appCount; i++)
            {
                var app = new YarnApp
                {
                    StartingClient = Client,
                    Parser = monitorParser,
                    FaultConnector = faultConnector,
                };

                Client.StartingYarnApps.Add(app);
                Applications.Add(app);
            }
        }

        /// <summary>
        /// Init application attempt instances
        /// </summary>
        /// <param name="attemptCount">Attempt instances count per app</param>
        /// <param name="monitorParser">Parser to use for monitoring</param>
        private void InitAppAttempts(int attemptCount, IHadoopParser monitorParser)
        {
            foreach(var app in Applications)
            {
                for(int i = 0; i < attemptCount; i++)
                {
                    var attempt = new YarnAppAttempt
                    {
                        App = app,
                        Parser = monitorParser,
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
        /// <param name="monitorParser">Parser to use for monitoring</param>
        private void InitContainers(int containerCount, IHadoopParser monitorParser)
        {
            foreach(var attempt in AppAttempts)
            {
                for(int i = 0; i < containerCount; i++)
                {
                    var container = new YarnAppContainer
                    {
                        AppAttempt = attempt,
                        Parser = monitorParser,
                    };

                    attempt.Containers.Add(container);
                    AppContainers.Add(container);
                }
            }
        }

        #endregion

    }
}