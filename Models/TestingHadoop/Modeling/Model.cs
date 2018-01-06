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
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling
{
    /// <summary>
    /// Hadoop model base class
    /// </summary>
    public class Model : ModelBase
    {
        #region Model size

        //public const int MaxApplicationCount = 0xFF;
        //public const int MaxAppAttemptCount = 0xF;
        //public const int MaxContainerCount = 0xF;

        #endregion

        #region Model components

        public Client Client { get; set; }
        public YarnController Controller { get; set; }
        public List<YarnNode> Nodes { get; } = new List<YarnNode>();

        public List<YarnApp> Applications { get; } = new List<YarnApp>();
        public List<YarnAppAttempt> AppAttempts { get; } = new List<YarnAppAttempt>();
        public List<YarnAppContainer> AppContainers { get; } = new List<YarnAppContainer>();

        #endregion

        #region Base methods

        /// <summary>
        /// Initializes a new model
        /// </summary>
        public Model()
        {

        }

        #endregion

        #region Configurations

        /// <summary>
        /// Default Config
        /// </summary>
        public void InitializeDefaultInstance()
        {

        }

        /// <summary>
        /// Only one pi calculation
        /// </summary>
        public void Config1()
        {
            InitBaseComponents();
            InitYarnNodes(4);

            InitApplications(1);
            InitAppAttempts(1);
            InitContainers(8);
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
        /// <param name="nodeCount"></param>
        private void InitYarnNodes(int nodeCount)
        {
            for (int i = 1; i <= nodeCount; i++)
            {
                var node = new YarnNode
                {
                    Name = "compute-" + i,
                    Controller = Controller,
                };

                Controller.ConnectedNodes.Add(node);
                Nodes.Add(node);
            }
        }

        /// <summary>
        /// Init application instances
        /// </summary>
        /// <param name="appCount">Instances count</param>
        private void InitApplications(int appCount)
        {
            for (int i = 0; i < appCount; i++)
            {
                var app = new YarnApp
                {
                    StartingClient = Client,
                };

                Client.StartingYarnApps.Add(app);
                Applications.Add(app);
            }
        }

        /// <summary>
        /// Init application attempt instances
        /// </summary>
        /// <param name="attemptCount">Attempt instances count per app</param>
        private void InitAppAttempts(int attemptCount)
        {
            foreach (var app in Applications)
            {
                for (int i = 0; i < attemptCount; i++)
                {
                    var attempt = new YarnAppAttempt
                    {
                        App = app,
                    };

                    app.AppAttempts.Add(attempt);
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
            foreach (var attempt in AppAttempts)
            {
                for (int i = 0; i < containerCount; i++)
                {
                    var container = new YarnAppContainer
                    {
                        YarnAppAttempt = attempt,
                    };

                    attempt.Containers.Add(container);
                    AppContainers.Add(container);
                }
            }
        }

        #endregion

    }
}