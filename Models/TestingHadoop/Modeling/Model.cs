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
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Connector;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Parser;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling
{
    /// <summary>
    /// Hadoop model base class
    /// </summary>
    public class Model : ModelBase
    {
        #region General settings

        /// <summary>
        /// Prefix for compute nodes
        /// </summary>
        public const string NodeNamePrefix = "compute-";

        /// <summary>
        /// Command for Hadoop setup script
        /// </summary>
        /// <remarks>
        /// Generic options for all cluster related commands can be inserted here.
        /// </remarks>
        public const string HadoopSetupScript = "/home/hadoop/hadoop-benchmark/setup.sh -q";

        /// <summary>
        /// Command for benchmark startup script
        /// </summary>
        public const string BenchmarkStartupScript = "/home/hadoop/hadoop-benchmark/bench.sh";

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

        public List<Client> Clients { get; set; }

        [Root(RootKind.Controller)]
        public YarnController Controller { get; set; }

        [Root(RootKind.Plant)]
        public Dictionary<string, YarnNode> Nodes { get; }

        [Root(RootKind.Plant)]
        public List<YarnApp> Applications { get; }

        [Root(RootKind.Plant)]
        public List<YarnAppAttempt> AppAttempts { get; }

        [Root(RootKind.Plant)]
        public List<YarnAppContainer> AppContainers { get; }

        #endregion

        #region Base methods

        /// <summary>
        /// Initializes a new model
        /// </summary>
        /// <param name="name">Model name</param>
        public Model(string name = "")
        {
            Name = name;

            Clients = new List<Client>();
            Nodes = new Dictionary<string, YarnNode>();
            Applications = new List<YarnApp>();
            AppAttempts = new List<YarnAppAttempt>();
            AppContainers = new List<YarnAppContainer>();
        }

        #endregion

        #region Configurations

        /// <summary>
        /// Initialize configuration for model testing
        /// </summary>
        /// <param name="parser">The monitorParser to use</param>
        /// <param name="connector">The faultConnector to use</param>
        public void InitTestConfig(IHadoopParser parser, IHadoopConnector connector)
        {
            if(Controller == null)
                InitController();

            InitYarnNodes(4, parser, connector);
            InitClients(1, parser, connector);
            InitApplications(8, parser, connector);
            InitAppAttempts(4, parser);
            InitContainers(32, parser);
        }


        /// <summary>
        /// Initizalizes the config with the given component counts
        /// </summary>
        /// <param name="clientCound">The client count to initialize</param>
        /// <param name="appCount">The application count per client to initialize</param>
        /// <param name="attemptCount">The attempt count per application to initialize</param>
        /// <param name="containerCount">The container count per attempt to initialize</param>
        /// <param name="nodeCount">The node count to initialize</param>
        public void InitModel(int clientCound = 1, int appCount = 16, int attemptCount = 4, int containerCount = 32, int nodeCount = 4)
        {
            InitController();

            var cmdConnector = new CmdConnector(SshHost, SshUsername, SshPrivateKeyFilePath, false, true, 1);
            var restConnector = new RestConnector(SshHost, SshUsername, SshPrivateKeyFilePath, Controller.HttpUrl, Controller.TimelineHttpUrl);
            var restParser = new RestParser(this, restConnector);
            Controller.Parser = restParser;

            InitYarnNodes(nodeCount, restParser, cmdConnector);
            InitClients(clientCound, restParser, cmdConnector);
            InitApplications(appCount, restParser, cmdConnector);
            InitAppAttempts(attemptCount, restParser);
            InitContainers(containerCount, restParser);
        }

        #endregion

        #region Component Inits

        /// <summary>
        /// Init hadoop controller
        /// </summary>
        private void InitController()
        {
            Controller = new YarnController("controller");
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
                var node = new YarnNode(NodeNamePrefix + i, monitorParser, faultConnector, Controller);

                Controller.ConnectedNodes.Add(node);
                Nodes[node.Name] = node;
            }
        }

        /// <summary>
        /// Init submitting clients
        /// </summary>
        /// <param name="clientCount">Instances count</param>
        /// <param name="monitorParser">Parser to use for monitoring</param>
        /// <param name="submittingConnector">Connector to submitting <see cref="YarnApp"/></param>
        private void InitClients(int clientCount, IHadoopParser monitorParser, IHadoopConnector submittingConnector)
        {
            for(int i = 0; i < clientCount; i++)
            {
                var client = new Client(Controller, monitorParser, submittingConnector, $"client{i}");

                Controller.ConnectedClients.Add(client);
                Clients.Add(client);
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
            foreach(var client in Clients)
            {
                for(int i = 0; i < appCount; i++)
                {
                    var app = new YarnApp(faultConnector, client, monitorParser);

                    client.Apps.Add(app);
                    Controller.Apps.Add(app);
                    Applications.Add(app);
                }
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
                    var attempt = new YarnAppAttempt(app, monitorParser);

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
                    var container = new YarnAppContainer(attempt, monitorParser);

                    attempt.Containers.Add(container);
                    AppContainers.Add(container);
                }
            }
        }

        #endregion

    }
}