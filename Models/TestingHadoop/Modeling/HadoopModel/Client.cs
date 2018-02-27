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
using System.Collections.Generic;
using System.Linq;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// <see cref="Client"/> for the Hadoop cluster
    /// </summary>
    public class Client : Component
    {

        #region Properties

        /// <summary>
        /// Started <see cref="YarnApp"/>s of the client
        /// </summary>
        public List<YarnApp> Apps { get; }

        /// <summary>
        /// Connected <see cref="YarnController"/> for the client
        /// </summary>
        public YarnController ConnectedYarnController { get; set; }

        /// <summary>
        /// Parser to monitoring data from cluster
        /// </summary>
        public IHadoopParser Parser { get; set; }

        /// <summary>
        /// The connector to submit a <see cref="YarnApp"/>
        /// </summary>
        public IHadoopConnector SubmittingConnector { get; set; }

        /// <summary>
        /// HDFS base directory for the client
        /// </summary>
        public string ClientDir { get; set; }

        /// <summary>
        /// The benchmark controller
        /// </summary>
        public BenchmarkController BenchController { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new empty <see cref="Client"/>
        /// </summary>
        public Client()
        {
            Apps = new List<YarnApp>();
        }

        /// <summary>
        /// Initializes a new <see cref="Client"/>
        /// </summary>
        /// <param name="controller">Connected <see cref="YarnController"/> for the client</param>
        /// <param name="parser">Parser to monitoring data from cluster</param>
        /// <param name="submittingConnector">The connector to submit a <see cref="YarnApp"/></param>
        /// <param name="clientHdfsDir">The hdfs base directory for this client</param>
        public Client(YarnController controller, IHadoopParser parser, IHadoopConnector submittingConnector, string clientHdfsDir)
            : this()
        {
            ConnectedYarnController = controller;
            Parser = parser;
            SubmittingConnector = submittingConnector;
            ClientDir = clientHdfsDir;

            BenchController = new BenchmarkController();
        }

        /// <summary>
        /// Initializes a new <see cref="Client"/>
        /// </summary>
        /// <param name="controller">Connected <see cref="YarnController"/> for the client</param>
        /// <param name="parser">Parser to monitoring data from cluster</param>
        /// <param name="submittingConnector">The connector to submit a <see cref="YarnApp"/></param>
        /// <param name="clientHdfsDir">The hdfs base directory for this client</param>
        /// <param name="benchControllerSeed">Seed for <see cref="BenchmarkController"/> transition system</param>
        public Client(YarnController controller, IHadoopParser parser, IHadoopConnector submittingConnector, string clientHdfsDir, int benchControllerSeed)
            : this(controller, parser, submittingConnector, clientHdfsDir)
        {
            BenchController = new BenchmarkController(benchControllerSeed);
        }

        #endregion

        #region General methods

        public override void Update()
        {
            MonitorApps();
            UpdateBenchmark();
        }

        #endregion

        #region App related methods

        /// <summary>
        /// Updates the current executing benchmark
        /// </summary>
        public void UpdateBenchmark()
        {
            bool benchChanged;
            if(!BenchController.IsInit)
            {
                BenchController.InitStartBench();
                benchChanged = true;
            }
            else
            {
                benchChanged = BenchController.ChangeBenchmark();
            }

            if(benchChanged)
            {
                StopBenchmarks();
                StartBenchmark(BenchController.CurrentBenchmark.GetStartCmd(ClientDir));
            }

        }

        /// <summary>
        /// Stops all currently running <see cref="Apps"/> from this <see cref="Client"/> and returns true on success
        /// </summary>
        /// <returns>True if all <see cref="Apps"/> are stopped</returns>
        public bool StopBenchmarks()
        {
            var success = true;
            foreach(var app in Apps)
            {
                var appSucc = app.StopApp();
                if(!appSucc)
                    success = false;
            }
            return success;
        }

        /// <summary>
        /// Starts the given benchmark command
        /// </summary>
        /// <param name="cmd">Command to start</param>
        public void StartBenchmark(string cmd)
        {
            SubmittingConnector.StartApplication(cmd);
        }

        /// <summary>
        /// Gets all apps executed on the cluster and their informations
        /// </summary>
        public void MonitorApps()
        {
            var parsedApps = Parser.ParseAppList(EAppState.ALL);
            foreach(var parsed in parsedApps)
            {
                var app = Apps.FirstOrDefault(a => a.AppId == parsed.AppId) ??
                          Apps.FirstOrDefault(a => String.IsNullOrWhiteSpace(a.AppId));
                if(app == null)
                    throw new OutOfMemoryException("No more applications available! Try to initialize more applications.");

                app.SetStatus(parsed);
                app.IsSelfMonitoring = false;
                app.MonitorStatus();
            }
        }

        #endregion

    }
}