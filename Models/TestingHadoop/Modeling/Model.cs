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
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Connector;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Parser;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling
{
    /// <summary>
    /// Hadoop model base class
    /// </summary>
    public class Model : ModelBase
    {
        #region Instance

        private static Model _Instance;

        /// <summary>
        /// <see cref="Model"/> instance
        /// </summary>
        public static Model Instance => _Instance ?? CreateNewInstance();

        #endregion

        #region Model components

        /// <summary>
        /// List of Clients
        /// </summary>
        [Root(RootKind.Plant)]
        public List<Client> Clients { get; }

        /// <summary>
        /// Hadoop Controller
        /// </summary>
        [Root(RootKind.Controller)]
        public YarnController Controller { get; private set; }

        /// <summary>
        /// Hadoop Nodes
        /// </summary>
        [Root(RootKind.Plant)]
        public List<YarnNode> Nodes { get; }

        /// <summary>
        /// All executed applications
        /// </summary>
        [Root(RootKind.Plant)]
        public List<YarnApp> Applications { get; }

        /// <summary>
        /// All application attempts
        /// </summary>
        [Root(RootKind.Plant)]
        public List<YarnAppAttempt> AppAttempts { get; }

        /// <summary>
        /// All current application containers
        /// </summary>
        [Root(RootKind.Plant)]
        public List<YarnAppContainer> AppContainers { get; }

        /// <summary>
        /// The <see cref="IHadoopConnector"/> in use
        /// </summary>
        [NonSerializable]
        public IHadoopConnector UsingFaultingConnector { get; private set; }

        /// <summary>
        /// The <see cref="IHadoopParser"/> in use
        /// </summary>
        [NonSerializable]
        public IHadoopParser UsingMonitoringParser { get; private set; }

        #endregion

        #region Base methods

        /// <summary>
        /// Initializes a new model
        /// </summary>
        private Model()
        {
            Clients = new List<Client>();
            Nodes = new List<YarnNode>();
            Applications = new List<YarnApp>();
            AppAttempts = new List<YarnAppAttempt>();
            AppContainers = new List<YarnAppContainer>();
        }

        /// <summary>
        /// Creates a new <see cref="Model"/> instance, saves and returns it
        /// </summary>
        /// <returns>The instance</returns>
        private static Model CreateNewInstance()
        {
            _Instance = new Model();

            return _Instance;
        }

        /// <summary>
        /// Resets the <see cref="Model"/> instance
        /// </summary>
        public static void ResetInstance()
        {
            _Instance = null;
        }

        #endregion

        #region Configurations

        /// <summary>
        /// Initialize configuration for model testing
        /// </summary>
        /// <param name="usingParser">The <see cref="IHadoopParser"/> to use</param>
        /// <param name="usingConnector">The <see cref="IHadoopConnector"/> to use</param>
        public void InitTestConfig(IHadoopParser usingParser, IHadoopConnector usingConnector)
        {
            if(Controller == null)
                InitController();

            UsingFaultingConnector = usingConnector;
            UsingMonitoringParser = usingParser;

            InitYarnNodes(4);
            InitClients(1);
            InitApplications(8);
            InitAppAttempts(4);
            InitContainers(32);
        }


        /// <summary>
        /// Initizalizes the config with the given component counts
        /// </summary>
        /// <param name="clientCount">The client count to initialize</param>
        /// <param name="appCount">The application count per client to initialize</param>
        /// <param name="attemptCount">The attempt count per application to initialize</param>
        /// <param name="containerCount">The container count per attempt to initialize</param>
        public void InitModel(int clientCount = 1, int appCount = 16, int attemptCount = 3, int containerCount = -1)
        {
            var nodeCount = ModelUtilities.GetFullNodeCount();

            if(containerCount < 0)
                containerCount = nodeCount * 8 + 3;

            InitController();

            UsingMonitoringParser = RestParser.Instance;
            UsingFaultingConnector = CmdConnector.Instance;

            //var submitterCount = (int)Math.Ceiling(clientCount * 1.5);
            //var cmdConnector = new CmdConnector(SshHosts, SshUsernames, SshPrivateKeyFiles, false, true, submitterCount);
            //var restConnector = new RestConnector(SshHosts, SshUsernames, SshPrivateKeyFiles, Controller.HttpUrl, Controller.TimelineHttpUrl);
            //var restParser = new RestParser(this, restConnector);
            //Controller.Parser = restParser;

            InitYarnNodes(nodeCount);
            InitClients(clientCount);
            InitApplications(appCount);
            InitAppAttempts(attemptCount);
            InitContainers(containerCount);
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
        private void InitYarnNodes(int nodeCount)
        {
            for(int i = 1; i <= nodeCount; i++)
            {
                var node = ModelSettings.HostMode == ModelSettings.EHostMode.DockerMachine
                    ? new YarnNode(ModelSettings.NodeNamePrefix + i, Controller)
                    : new YarnNode(ModelSettings.NodeNamePrefix + i, Controller, $"{ModelSettings.NodeHttpUrlMultihostBase}:{8041 + i}");

                //Controller.ConnectedNodes.Add(node);
                Nodes.Add(node);
            }
        }

        /// <summary>
        /// Init submitting clients
        /// </summary>
        /// <param name="clientCount">Instances count</param>
        private void InitClients(int clientCount)
        {
            for(int i = 1; i <= clientCount; i++)
            {
                var client = new Client($"client{i}", ModelSettings.RandomBaseSeed + i);

                //Controller.ConnectedClients.Add(client);
                Clients.Add(client);
            }
        }

        /// <summary>
        /// Init application instances
        /// </summary>
        /// <param name="appCount">Instances count</param>
        private void InitApplications(int appCount)
        {
            foreach(var client in Clients)
            {
                for(int i = 0; i < appCount; i++)
                {
                    var app = new YarnApp(client);

                    //client.Apps.Add(app);
                    //Controller.Apps.Add(app);
                    Applications.Add(app);
                }
            }
        }

        /// <summary>
        /// Init application attempt instances
        /// </summary>
        /// <param name="attemptCount">Attempt instances count per app</param>
        private void InitAppAttempts(int attemptCount)
        {
            foreach(var app in Applications)
            {
                for(int i = 0; i < attemptCount; i++)
                {
                    var attempt = new YarnAppAttempt();

                    app.Attempts.Add(attempt);
                    AppAttempts.Add(attempt);
                }
            }
        }

        /// <summary>
        /// Init application container instances
        /// </summary>
        /// <param name="containerCount">Container instances count per attempt</param>
        private void InitContainers(int containerCount)
        {
            foreach(var attempt in AppAttempts)
            {
                for(int i = 0; i < containerCount; i++)
                {
                    var container = new YarnAppContainer();

                    attempt.Containers.Add(container);
                    AppContainers.Add(container);
                }
            }
        }

        #endregion

    }
}