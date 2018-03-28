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
using SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Parser;
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
        /// Started <see cref="CurrentExecutingApp"/>s of the client
        /// </summary>
        public List<YarnApp> Apps { get; }

        /// <summary>
        /// The current executing application
        /// </summary>
        public YarnApp CurrentExecutingApp { get; private set; }

        /// <summary>
        /// Connected <see cref="YarnController"/> for the client
        /// </summary>
        public YarnController ConnectedYarnController { get; set; }

        /// <summary>
        /// Parser to monitoring data from cluster
        /// </summary>
        [NonSerializable]
        public IHadoopParser Parser => RestParser.Instance;

        /// <summary>
        /// The connector to submit a <see cref="CurrentExecutingApp"/>
        /// </summary>
        [NonSerializable]
        public IHadoopConnector SubmittingConnector => Model.UsingFaultingConnector;

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
        /// <param name="clientHdfsDir">The hdfs base directory for this client</param>
        public Client(YarnController controller, string clientHdfsDir)
            : this()
        {
            ConnectedYarnController = controller;
            ClientDir = clientHdfsDir;

            BenchController = new BenchmarkController();
        }

        /// <summary>
        /// Initializes a new <see cref="Client"/>
        /// </summary>
        /// <param name="controller">Connected <see cref="YarnController"/> for the client</param>
        /// <param name="clientHdfsDir">The hdfs base directory for this client</param>
        /// <param name="benchControllerSeed">Seed for <see cref="BenchmarkController"/> transition system</param>
        public Client(YarnController controller, string clientHdfsDir, int benchControllerSeed)
            : this(controller, clientHdfsDir)
        {
            BenchController = new BenchmarkController(benchControllerSeed);
        }

        #endregion

        #region General methods

        public override void Update()
        {
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
                StopCurrentBenchmark();
                StartBenchmark(BenchController.CurrentBenchmark);
            }

        }

        /// <summary>
        /// Stops the <see cref="YarnApp"/> stored in <see cref="CurrentExecutingApp"/> and resets the property to null
        /// </summary>
        /// <returns>True if <see cref="CurrentExecutingApp"/> is stopped or null</returns>
        public bool StopCurrentBenchmark()
        {
            var isStopped = CurrentExecutingApp?.StopApp();
            CurrentExecutingApp = null;
            return isStopped ?? true;
        }

        /// <summary>
        /// Starts the given benchmark and save the application id in the first available
        /// <see cref="YarnApp"/> in <see cref="Apps"/> and this in <see cref="CurrentExecutingApp"/>.
        /// If benchmark cannot be started, <see cref="CurrentExecutingApp"/> will be set to null.
        /// If an application id was saved it will returned, else <see cref="String.Empty"/>.
        /// </summary>
        /// <param name="benchmark">Benchmark to start</param>
        /// <exception cref="OutOfMemoryException">No <see cref="YarnApp"/> available</exception>
        /// <returns>The submitted application id</returns>
        public string StartBenchmark(Benchmark benchmark)
        {
            if(benchmark.HasOutputDir)
                SubmittingConnector.RemoveHdfsDir(benchmark.GetOutputDir(ClientDir));
            var appId = SubmittingConnector.StartApplicationAsync(benchmark.GetStartCmd(ClientDir));

            if(appId.Length <= 32)
            {
                var app = Apps.FirstOrDefault(a => String.IsNullOrWhiteSpace(a.AppId));
                if(app == null)
                    throw new OutOfMemoryException("No more applications available! Try to initialize more applications.");
                app.AppId = appId;
                CurrentExecutingApp = app;
                return CurrentExecutingApp.AppId;
            }

            CurrentExecutingApp = null;
            return String.Empty;
        }

        #endregion

    }
}