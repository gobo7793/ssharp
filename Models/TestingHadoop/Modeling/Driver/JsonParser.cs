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
using Newtonsoft.Json;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    public class JsonParser : IHadoopParser
    {
        #region Properties and Constants
        
        /// <summary>
        /// Model with its components
        /// </summary>
        public Model Model { get; }

        /// <summary>
        /// The connection to Hadoop
        /// </summary>
        public IHadoopConnector Connection { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the Parser with the given <see cref="Modeling.Model"/> and its components
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="connection">The connector to Hadoop</param>
        public JsonParser(Model model, IHadoopConnector connection)
        {
            Model = model;
            Connection = connection;
        }

        #endregion

        #region IHadoopParser

        /// <summary>
        /// Gets and parses the <see cref="YarnApp"/> list for the given <see cref="EAppState"/>s.
        /// </summary>
        /// <param name="states">The states</param>
        /// <returns>The applications</returns>
        public ApplicationResult[] ParseAppList(EAppState states = EAppState.None)
        {
            var appStates = ParserUtilities.ConcatStates(states);

            var fullResult = Connection.GetYarnApplicationList(appStates);
            var appRes = JsonConvert.DeserializeObject<ApplicationListJsonResult>(fullResult);

            // convert AM Hosts
            var apps = appRes.Collection.List;
            foreach(var app in apps)
                app.AmHost = ParserUtilities.ParseNode(app.AmHostHttpAddress, Model);

            return apps;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppAttempt"/> list for the given <see cref="YarnApp.AppId"/>
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The attempts</returns>
        public ApplicationAttemptResult[] ParseAppAttemptList(string appId)
        {
            var fullResult = Connection.GetYarnAppAttemptList(appId);
            var attemptRes = JsonConvert.DeserializeObject<JsonAppAttemptListResult>(fullResult);

            // convert AM Hosts
            var attempts = attemptRes.Collection.List;
            foreach(var attempt in attempts)
                attempt.AmHost = ParserUtilities.ParseNode(attempt.AmHostId, Model);

            return attempts;
        }

        /// <summary>
        /// Gets and parses the current running <see cref="YarnAppContainer"/> list for the given <see cref="YarnAppAttempt.AttemptId"/>
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The running containers</returns>
        public ContainerResult[] ParseContainerList(string attemptId)
        {
            throw new NotImplementedException();
        }
        
        public ApplicationResult ParseAppDetails(string appId)
        {
            var fullResult = Connection.GetYarnApplicationDetails(appId);
            var app = JsonConvert.DeserializeObject<ApplicationDetailsJsonResult>(fullResult).App;

            // convert AM Hosts
            app.AmHost = ParserUtilities.ParseNode(app.AmHostHttpAddress, Model);

            return app;
        }

        public ApplicationAttemptResult ParseAppAttemptDetails(string attemptId)
        {
            throw new NotImplementedException();
        }

        public ContainerResult ParseContainerDetails(string containerId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnNode"/> list for the cluster
        /// </summary>
        /// <returns>All nodes in the cluster</returns
        public NodeResult[] ParseNodeList()
        {
            var fullResult = Connection.GetYarnNodeList();
            var nodeRes = JsonConvert.DeserializeObject<NodeListJsonResult>(fullResult);
            
            return nodeRes.Collection.List;
        }

        public NodeResult ParseNodeDetails(string nodeId)
        {
            var fullResult = Connection.GetYarnNodeDetails(nodeId);
            var node = JsonConvert.DeserializeObject<NodeDetailsJsonResult>(fullResult).Node;

            return node;
        }

        #endregion
    }
}