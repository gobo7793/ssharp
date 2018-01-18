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
        /// Hadoop converted timestamp format
        /// </summary>
        public const string HadoopDateFormat = "ddd MMM dd HH:mm:ss zz00 yyyy";

        /// <summary>
        /// Generic regex for lists
        /// </summary>
        /// <remarks>
        /// Group 1: the property without leading or trailing whitespaces
        /// </remarks>
        private static readonly Regex _GenericListRegex = new Regex(@"\s*([^\t]+)");

        /// <summary>
        /// Generic regex for details
        /// </summary>
        /// <remarks>
        /// Group 1: the property name without leading or trailing whitespaces
        /// Group 2: the property value without leading or trailing whitespaces
        /// </remarks>
        private static readonly Regex _GenericDetailsRegex = new Regex(@"^\t(.+)\s:\s(.*)$", RegexOptions.Multiline);

        /// <summary>
        /// Line splitter regex
        /// </summary>
        private static readonly Regex _LineSplitterRegex = new Regex(@"\r\n|\r|\n");

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

            var appList = new List<ApplicationResult>();
            var resLines = _LineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var appMatches = _GenericListRegex.Matches(resLine);
                if(appMatches.Count != 9 || !appMatches[0].Groups[1].Value.StartsWith("application"))
                    continue;

                var app = new ApplicationResult
                {
                    AppId = appMatches[0].Groups[1].Value,
                    AppName = appMatches[1].Groups[1].Value,
                    AppType = appMatches[2].Groups[1].Value,
                    State = ParserUtilities.ParseAppState(appMatches[5].Groups[1].Value),
                    FinalStatus = ParserUtilities.ParseFinalStatus(appMatches[6].Groups[1].Value),
                    Progess = ParserUtilities.ParseIntText(appMatches[7].Groups[1].Value),
                    TrackingUrl = appMatches[8].Groups[1].Value,
                };

                appList.Add(app);
            }

            return appList.ToArray();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppAttempt"/> list for the given <see cref="YarnApp.AppId"/>
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The attempts</returns>
        public ApplicationAttemptResult[] ParseAppAttemptList(string appId)
        {
            var fullResult = Connection.GetYarnAppAttemptList(appId);

            var attemptList = new List<ApplicationAttemptResult>();
            var resLines = _LineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var attemptMatches = _GenericListRegex.Matches(resLine);
                if(attemptMatches.Count != 4 || !attemptMatches[0].Groups[1].Value.StartsWith("appattempt"))
                    continue;

                var attempt = new ApplicationAttemptResult
                {
                    AttemptId = attemptMatches[0].Groups[1].Value,
                    State = ParserUtilities.ParseAppState(attemptMatches[1].Groups[1].Value),
                    AmContainerId = attemptMatches[2].Groups[1].Value,
                    TrackingUrl = attemptMatches[3].Groups[1].Value,
                };

                attemptList.Add(attempt);
            }

            return attemptList.ToArray();
        }

        /// <summary>
        /// Gets and parses the current running <see cref="YarnAppContainer"/> list for the given <see cref="YarnAppAttempt.AttemptId"/>
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The running containers</returns>
        public ContainerResult[] ParseContainerList(string attemptId)
        {
            var fullResult = Connection.GetYarnAppContainerList(attemptId);

            var containerList = new List<ContainerResult>();
            var resLines = _LineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var containerMatches = _GenericListRegex.Matches(resLine);
                if(containerMatches.Count != 7 || !containerMatches[0].Groups[1].Value.StartsWith("container"))
                    continue;

                var container = new ContainerResult
                {
                    ContainerId = containerMatches[0].Groups[1].Value,
                    StartTime = ParserUtilities.ParseJavaTimestamp(containerMatches[1].Groups[1].Value, HadoopDateFormat),
                    FinishTime = ParserUtilities.ParseJavaTimestamp(containerMatches[2].Groups[1].Value, HadoopDateFormat),
                    State = ParserUtilities.ParseContainerState(containerMatches[3].Groups[1].Value),
                    Host = ParserUtilities.ParseNode(containerMatches[4].Groups[1].Value, Model),
                    LogUrl = containerMatches[6].Groups[1].Value,
                };

                containerList.Add(container);
            }

            return containerList.ToArray();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnApp"/> details, returns null if error appears
        /// </summary>
        /// <param name="appId">The <see cref="YarnApp.AppId"/> from the app</param>
        /// <returns>The application details or null on errors</returns>
        public ApplicationResult ParseAppDetails(string appId)
        {
            var fullResult = Connection.GetYarnApplicationDetails(appId);

            var matches = _GenericDetailsRegex.Matches(fullResult);

            if(matches.Count == 15)
            {
                var resourcesMatches = Regex.Matches(matches[13].Groups[2].Value, @"\d+");
                var app = new ApplicationResult
                {
                    AppId = matches[0].Groups[2].Value,
                    AppName = matches[1].Groups[2].Value,
                    AppType = matches[2].Groups[2].Value,
                    StartTime = ParserUtilities.ParseJavaTimestamp(matches[5].Groups[2].Value, null),
                    FinishTime = ParserUtilities.ParseJavaTimestamp(matches[6].Groups[2].Value, null),
                    Progess = ParserUtilities.ParseIntText(matches[7].Groups[2].Value),
                    State = ParserUtilities.ParseAppState(matches[8].Groups[2].Value),
                    FinalStatus = ParserUtilities.ParseFinalStatus(matches[9].Groups[2].Value),
                    TrackingUrl = matches[10].Groups[2].Value,
                    AmHost = ParserUtilities.ParseNode(matches[12].Groups[2].Value, Model),
                    MbSeconds = ParserUtilities.ParseInt(resourcesMatches[0].Value),
                    VcoreSeconds = ParserUtilities.ParseInt(resourcesMatches[1].Value),
                };

                return app;
            }

            return null;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppAttempt"/> details, returns null if error appears
        /// </summary>
        /// <param name="attemptId">The <see cref="YarnAppAttempt.AttemptId"/> from the attempt</param>
        /// <returns>The attempt details or null on errors</returns>
        public ApplicationAttemptResult ParseAppAttemptDetails(string attemptId)
        {
            var fullResult = Connection.GetYarnAppAttemptDetails(attemptId);

            var matches = _GenericDetailsRegex.Matches(fullResult);

            if(matches.Count == 7)
            {
                var attempt = new ApplicationAttemptResult
                {
                    AttemptId = matches[0].Groups[2].Value,
                    State = ParserUtilities.ParseAppState(matches[1].Groups[2].Value),
                    AmContainerId = matches[2].Groups[2].Value,
                    TrackingUrl = matches[3].Groups[2].Value,
                    AmHost = ParserUtilities.ParseNode(matches[5].Groups[2].Value, Model),
                };

                return attempt;
            }

            return null;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppContainer"/> details, returns null if error appears
        /// </summary>
        /// <param name="containerId">The <see cref="YarnAppContainer.ContainerId"/> from the container</param>
        /// <returns>The container details or null on errors</returns>
        public ContainerResult ParseContainerDetails(string containerId)
        {
            var fullResult = Connection.GetYarnAppContainerDetails(containerId);

            var matches = _GenericDetailsRegex.Matches(fullResult);

            if(matches.Count == 8)
            {
                var container = new ContainerResult
                {
                    ContainerId = matches[0].Groups[2].Value,
                    StartTime = ParserUtilities.ParseJavaTimestamp(matches[1].Groups[2].Value, null),
                    FinishTime = ParserUtilities.ParseJavaTimestamp(matches[2].Groups[2].Value, null),
                    State = ParserUtilities.ParseContainerState(matches[3].Groups[2].Value),
                    LogUrl = matches[4].Groups[2].Value,
                    Host = ParserUtilities.ParseNode(matches[5].Groups[2].Value, Model),
                };

                return container;
            }

            return null;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnNode"/> list for the cluster
        /// </summary>
        /// <returns>All nodes in the cluster</returns>
        public NodeResult[] ParseNodeList()
        {
            var fullResult = Connection.GetYarnNodeList();

            var nodeList = new List<NodeResult>();
            var resLines = _LineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var nodeMatches = _GenericListRegex.Matches(resLine);
                if(nodeMatches.Count != 4 || !nodeMatches[0].Groups[1].Value.StartsWith(Model.NodeNamePrefix))
                    continue;

                var node = new NodeResult
                {
                    NodeId = nodeMatches[0].Groups[1].Value,
                    NodeState = ParserUtilities.ParseNodeState(nodeMatches[1].Groups[1].Value),
                    NodeHttpAdd = nodeMatches[2].Groups[1].Value,
                    RunningContainerCount = ParserUtilities.ParseInt(nodeMatches[3].Groups[1].Value),
                };

                nodeList.Add(node);
            }

            return nodeList.ToArray();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnNode"/> details, returns null if error appears
        /// </summary>
        /// <param name="nodeId">The <see cref="YarnNode.NodeId"/> from the node</param>
        /// <returns>The node details or null on errors</returns>
        public NodeResult ParseNodeDetails(string nodeId)
        {
            var fullResult = Connection.GetYarnNodeDetails(nodeId);

            var matches = _GenericDetailsRegex.Matches(fullResult);

            if(matches.Count == 12)
            {
                var node = new NodeResult
                {
                    NodeId = matches[0].Groups[2].Value,
                    NodeState = ParserUtilities.ParseNodeState(matches[2].Groups[2].Value),
                    NodeHttpAdd = matches[3].Groups[2].Value,
                    RunningContainerCount = ParserUtilities.ParseInt(matches[6].Groups[2].Value),
                    MemoryUsed = ParserUtilities.ParseIntText(matches[7].Groups[2].Value),
                    MemoryCapacity = ParserUtilities.ParseIntText(matches[8].Groups[2].Value),
                    CpuUsed = ParserUtilities.ParseIntText(matches[9].Groups[2].Value),
                    CpuCapacity = ParserUtilities.ParseIntText(matches[10].Groups[2].Value),
                };

                return node;
            }

            return null;
        }

        #endregion
    }
}

