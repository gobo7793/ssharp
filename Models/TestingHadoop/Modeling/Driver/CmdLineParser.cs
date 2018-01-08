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
using System.Text.RegularExpressions;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    /// <summary>
    /// Parser for informations about Hadoop states via command line
    /// </summary>
    public class CmdLineParser : IHadoopParser
    {
        #region Constants and Properties

        /// <summary>
        /// Generic regex pattern for lists
        /// </summary>
        private static readonly Regex _genericListRegex = new Regex(@"[\S]+");

        /// <summary>
        /// Line splitter regex pattern
        /// </summary>
        private static readonly Regex _lineSplitterRegex = new Regex(@"\r\n|\r|\n");

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
        public CmdLineParser(Model model, IHadoopConnector connection)
        {
            Model = model;
            Connection = connection;
        }

        private string GetAppStateString(params EAppState[] states)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region IHadoopParser

        /// <summary>
        /// Gets and parses the <see cref="YarnApp"/> list for the given <see cref="EAppState"/>s.
        /// </summary>
        /// <param name="states">The states</param>
        /// <returns>The applications</returns>
        public ApplicationListResult[] ParseAppList(params EAppState[] states)
        {
            var appStates = GetAppStateString(states);

            var fullResult = Connection.GetYarnApplicationList(appStates);

            var appList = new List<ApplicationListResult>();
            var resLines = _lineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var appMatches = _genericListRegex.Matches(resLine);
                if(appMatches.Count != 9 || !appMatches[0].Value.StartsWith("application"))
                    continue;

                EAppState state;
                Enum.TryParse(appMatches[5].Value, true, out state);
                int progress;
                Int32.TryParse(appMatches[7].Value.Substring(0, appMatches[7].Value.Length - 1), out progress);

                var app = new ApplicationListResult(appMatches[0].Value, appMatches[1].Value, appMatches[2].Value, state,
                    appMatches[6].Value, progress, appMatches[8].Value);

                appList.Add(app);
            }

            return appList.ToArray();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppAttempt"/> list for the given <see cref="YarnApp.AppId"/>
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The attempts</returns>
        public ApplicationAttemptListResult[] ParseAttemptAttemptList(string appId)
        {
            var fullResult = Connection.GetYarnAppAttemptList(appId);

            var attemptList = new List<ApplicationAttemptListResult>();
            var resLines = _lineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var attemptMatches = _genericListRegex.Matches(resLine);
                if(attemptMatches.Count != 4 || !attemptMatches[0].Value.StartsWith("appattempt"))
                    continue;

                EAppState state;
                Enum.TryParse(attemptMatches[1].Value, true, out state);

                var attempt = new ApplicationAttemptListResult(attemptMatches[0].Value, state, attemptMatches[2].Value, attemptMatches[3].Value);

                attemptList.Add(attempt);
            }

            return attemptList.ToArray();
        }

        /// <summary>
        /// Gets and parses the current running <see cref="YarnAppContainer"/> list for the given <see cref="YarnAppAttempt.AttemptId"/>
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The running containers</returns>
        public ContainerListResult[] ParseContainerList(string attemptId)
        {
            var fullResult = Connection.GetYarnAppAttemptList(attemptId);

            var containerList = new List<ContainerListResult>();
            var resLines = _lineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var containerDetails = Regex.Split(resLine, @"\s{2,}");
                if(containerDetails.Length != 7 || !containerDetails[0].StartsWith("container"))
                    continue;

                DateTime startTime;
                DateTime.TryParse(containerDetails[1], out startTime);
                DateTime finishTime;
                DateTime.TryParse(containerDetails[2], out finishTime);
                EAppState state;
                Enum.TryParse(containerDetails[3], true, out state);
                var nodeName = containerDetails[4].Split(':')[0];
                var node = Model.Nodes.Find(x => x.Name == nodeName);

                var container = new ContainerListResult(containerDetails[0], startTime, finishTime, state, node, containerDetails[6]);

                containerList.Add(container);
            }

            return containerList.ToArray();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnApp"/> details
        /// </summary>
        /// <param name="appId">The <see cref="YarnApp.AppId"/> from the app</param>
        /// <returns>The application details</returns>
        public ApplicationDetailsResult ParseAppDetails(string appId)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppAttempt"/> details
        /// </summary>
        /// <param name="attemptId">The <see cref="YarnAppAttempt.AttemptId"/> from the attempt</param>
        /// <returns>The attempt details</returns>
        public ApplicationAttemptDetailsResult ParseAttemptAppDetails(string attemptId)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppContainer"/> details
        /// </summary>
        /// <param name="containerId">The <see cref="YarnAppContainer.ContainerId"/> from the container</param>
        /// <returns>The container details</returns>
        public ContainerListResult ParseContainerDetails(string containerId)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnNode"/> list for the cluster
        /// </summary>
        /// <returns>All nodes in the cluster</returns>
        public NodeListResult ParseNodeList()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnNode"/> details
        /// </summary>
        /// <param name="nodeId">The <see cref="YarnNode.NodeId"/> from the node</param>
        /// <returns>The node details</returns>
        public NodeDetailsResult ParseNodeDetails(string nodeId)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}