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
using System.Linq;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel;
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
        #region EHostMode

        /// <summary>
        /// Enum for possible host modes for this model
        /// </summary>
        public enum EHostMode
        {
            /// <summary>
            /// Classical mode that the cluster is started inside a docker-machine envoiroment,
            /// provided by hadoop-benchmark. The docker-machines are realized by virtualbox as VMs.
            /// One machine is for the consul, one for the controller and benchmark containers and
            /// each compute has its own VM.
            /// </summary>
            DockerMachine,

            /// <summary>
            /// Alternative mode that the cluster containers are directly started on the cluster pc
            /// without using docker-machine. The hadoop containers (one for controller and for each
            /// compute) are inside a docker swarm so it's possible to run the cluster on multiple hosts.
            /// </summary>
            Multihost
        }

        #endregion

        #region General settings and constants

        private static Model _Instance;
        private static bool _IsPrecreateBenchInputs;

        /// <summary>
        /// <see cref="Model"/> instance
        /// </summary>
        public static Model Instance => _Instance ?? CreateNewInstance();

        /// <summary>
        /// Prefix for compute nodes
        /// </summary>
        public const string NodeNamePrefix = "compute-";

        #region SSH

        /// <summary>
        /// Hostname for the Hadoop cluster pc
        /// </summary>
        public const string SshHost = "swtse143.informatik.uni-augsburg.de";

        /// <summary>
        /// Username for the Hadoop cluster pc
        /// </summary>
        public const string SshUsername = "hadoop";

        /// <summary>
        /// Full file path to the private key file to login
        /// </summary>
        public const string SshPrivateKeyFile = @"%USERPROFILE%\.ssh\id_rsa";

        #endregion

        #region Host mode

        /// <summary>
        /// The host mode of the Hadoop cluster on the cluster pc
        /// </summary>
        public static EHostMode HostMode { get; set; } = EHostMode.DockerMachine;

        /// <summary>
        /// Command for benchmark startup script
        /// </summary>
        /// <remarks>
        /// Generic options for all benchmark related commands can be inserted here.
        /// </remarks>
        public const string BenchmarkStartupScript = "/home/hadoop/hadoop-benchmark/bench.sh -q -t";

        /// <summary>
        /// The Hadoop setup script for use on classic singlehost
        /// mode with docker-machine VMs for the cluster
        /// </summary>
        /// <remarks>
        /// Generic options for all cluster related commands can be inserted here.
        /// </remarks>
        public const string HadoopSetupScriptDockerMachine = "/home/hadoop/hadoop-benchmark/setup.sh -q";

        /// <summary>
        /// The Hadoop setup script for use on multihost mode execution docker container
        /// directly on pc without docker-machine and in swarm mode
        /// </summary>
        /// <remarks>
        /// Generic options for all cluster related commands can be inserted here.
        /// </remarks>
        public const string HadoopSetupScriptMultihost = "/home/hadoop/hadoop-benchmark/multihost.sh -q";

        /// <summary>
        /// Command for Hadoop setup script that will be used in the model
        /// </summary>
        public static string HadoopSetupScript =>
            HostMode == EHostMode.DockerMachine ? HadoopSetupScriptDockerMachine : HadoopSetupScriptMultihost;

        /// <summary>
        /// Hadoop controller resource manager url for REST API on <see cref="SshHost"/>
        /// on use with docker-machine cluster
        /// </summary>
        public const string ControllerRestRmUrlDockerMachine = "http://controller:8088";

        /// <summary>
        /// Hadoop controller resource manager url for REST API on <see cref="SshHost"/>
        /// on use with multihost docker cluster
        /// </summary>
        public const string ControllerRestRmUrlMultihost = "http://localhost:8088";

        /// <summary>
        /// Hadoop controller resource manager url for REST API on <see cref="SshHost"/>
        /// that will be used in the model
        /// </summary>
        public static string ControllerRestRmUrl =>
            HostMode == EHostMode.DockerMachine ? ControllerRestRmUrlDockerMachine : ControllerRestRmUrlMultihost;

        /// <summary>
        /// Hadoop controller timeline server url for REST API on <see cref="SshHost"/>
        /// on use with docker-machine cluster
        /// </summary>
        public const string ControllerRestTlsUrlDockerMachine = "http://controller:8188";

        /// <summary>
        /// Hadoop controller timeline server url for REST API on <see cref="SshHost"/>
        /// on use with multihost docker cluster
        /// </summary>
        public const string ControllerRestTlsUrlMultihost = "http://localhost:8188";

        /// <summary>
        /// Hadoop controller timeline server url for REST API on <see cref="SshHost"/>
        /// that will be used in the model
        /// </summary>
        public static string ControllerRestTlsUrl =>
            HostMode == EHostMode.DockerMachine ? ControllerRestTlsUrlDockerMachine : ControllerRestTlsUrlMultihost;

        /// <summary>
        /// Hadoop compute node base http url (url without port number)
        /// </summary>
        public const string NodeHttpUrlBase = "http://localhost";

        #endregion

        #region Benchmarks

        /// <summary>
        /// Base directory for precreated input data for benchmarks
        /// </summary>
        public const string PrecreateBenchInputsBaseDir = "InputData";

        /// <summary>
        /// Indicates if the benchmark input data is precreated before model execution, default false.
        /// Precreated data will be saved into <see cref="PrecreateBenchInputsBaseDir"/> and
        /// used as input data for benchmarks.
        /// </summary>
        public static bool IsPrecreateBenchInputs
        {
            get { return _IsPrecreateBenchInputs; }
            set
            {
                if(value)
                    BenchmarkController.PrecreateInputData();
                _IsPrecreateBenchInputs = value;
            }
        }

        #endregion

        #region Char array lengths

        public const int AppIdLength = 30;
        public const int AppNameLength = 0x7F;
        public const int AppAttemptIdLength = 36;
        public const int ContainerIdLength = 38;

        public const int HostNameLength = 10;
        public const int NodeIdLength = 16;
        public const int HttpUrlLength = 24;

        public const int ClientIdLength = 8;

        public const int TrackingUrlLength = 0x7F;
        public const int DiagnosticsLength = 0xFF;

        #endregion

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
        public static IHadoopConnector UsingFaultingConnector { get; private set; }

        /// <summary>
        /// The <see cref="IHadoopParser"/> in use
        /// </summary>
        [NonSerializable]
        public static IHadoopParser UsingMonitoringParser { get; private set; }

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

        #endregion

        #region Configurations

        /// <summary>
        /// Initialize configuration for model testing
        /// </summary>
        /// <param name="usingParser">The <see cref="IHadoopParser"/> to use</param>
        /// <param name="usingConnector">The <see cref="IHadoopConnector"/> to use</param>
        /// <param name="benchTransitionSeed">Seed for <see cref="BenchmarkController"/> transition system</param>
        public void InitTestConfig(IHadoopParser usingParser, IHadoopConnector usingConnector, int benchTransitionSeed = 1)
        {
            if(Controller == null)
                InitController();

            UsingFaultingConnector = usingConnector;
            UsingMonitoringParser = usingParser;

            InitYarnNodes(4);
            InitClients(1, benchTransitionSeed);
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
        /// <param name="nodeCount">The node count to initialize</param>
        /// <param name="benchTransitionSeed">Seed for <see cref="BenchmarkController"/> transition system</param>
        public void InitModel(int clientCount = 1, int appCount = 16, int attemptCount = 4, int containerCount = 32, int nodeCount = 4, int benchTransitionSeed = 1)
        {
            InitController();

            UsingMonitoringParser = RestParser.Instance;
            UsingFaultingConnector = CmdConnector.Instance;

            //var submitterCount = (int)Math.Ceiling(clientCount * 1.5);
            //var cmdConnector = new CmdConnector(SshHost, SshUsername, SshPrivateKeyFile, false, true, submitterCount);
            //var restConnector = new RestConnector(SshHost, SshUsername, SshPrivateKeyFile, Controller.HttpUrl, Controller.TimelineHttpUrl);
            //var restParser = new RestParser(this, restConnector);
            //Controller.Parser = restParser;

            InitYarnNodes(nodeCount);
            InitClients(clientCount, benchTransitionSeed);
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
                var node = HostMode == EHostMode.DockerMachine
                    ? new YarnNode(NodeNamePrefix + i, Controller)
                    : new YarnNode(NodeNamePrefix + i, Controller, $"{NodeHttpUrlBase}:{8041 + i}");

                //Controller.ConnectedNodes.Add(node);
                Nodes.Add(node);
            }
        }

        /// <summary>
        /// Init submitting clients
        /// </summary>
        /// <param name="clientCount">Instances count</param>
        /// <param name="benchTransitionSeed">Seed for <see cref="BenchmarkController"/> transition system</param>
        private void InitClients(int clientCount, int benchTransitionSeed)
        {
            for(int i = 1; i <= clientCount; i++)
            {
                var client = new Client($"client{i}", benchTransitionSeed);

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