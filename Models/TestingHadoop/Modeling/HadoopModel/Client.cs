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
        public List<YarnApp> StartingYarnApps { get; } = new List<YarnApp>();

        /// <summary>
        /// Connected <see cref="YarnController"/> for the client
        /// </summary>
        public YarnController ConnectedYarnController { get; }

        /// <summary>
        /// The connector to submit a <see cref="YarnApp"/>
        /// </summary>
        public IHadoopConnector SubmittingConnector { get; }

        /// <summary>
        /// The benchmark controller
        /// </summary>
        public BenchmarkController BenchController { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new client for the cluster
        /// </summary>
        /// <param name="clientHdfsDir">The hdfs base directory for this client</param>
        /// <param name="controller">Connected <see cref="YarnController"/> for the client</param>
        /// <param name="submittingConnector">The connector to submit a <see cref="YarnApp"/></param>
        public Client(string clientHdfsDir, YarnController controller, IHadoopConnector submittingConnector)
        {
            BenchController = new BenchmarkController(clientHdfsDir);
            BenchController.InitStartBench();
            ConnectedYarnController = controller;
            SubmittingConnector = submittingConnector;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the given <see cref="YarnApp"/>
        /// if <see cref="YarnApp.StartName"/> is not null
        /// </summary>
        /// <param name="app"><see cref="YarnApp"/> to start</param>
        public void StartJob(YarnApp app)
        {
            if(!String.IsNullOrWhiteSpace(app.StartName))
                SubmittingConnector.StartApplication(app.StartName, app.StartArgs);
        }

        /// <summary>
        /// Starts the given command
        /// </summary>
        /// <param name="cmd">Command to start</param>
        /// <param name="args">The arguments</param>
        public void StartJob(string cmd, string args)
        {
            SubmittingConnector.StartApplication(cmd, args);
        }

        #endregion

    }
}