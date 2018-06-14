﻿#region License
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
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Connector;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.DataClasses;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Parser
{
    /// <summary>
    /// Parser for informations about Hadoop states via command line
    /// </summary>
    public class CmdParser : IHadoopParser
    {
        #region Constants and Properties

        private static CmdParser _Instance;

        /// <summary>
        /// <see cref="CmdParser"/> instance
        /// </summary>
        public static CmdParser Instance => _Instance ?? CreateInstance();

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
        private static readonly Regex _GenericDetailsRegex = new Regex(@"\t(.+)\s:\s([^\t]*)[\n\r]", RegexOptions.Multiline);

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

        /// <summary>
        /// Maximum container count to parse on container list
        /// </summary>
        public int MaxContainerCount { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the Parser with the given <see cref="Modeling.Model"/> and its components
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="connection">The connector to Hadoop</param>
        /// <param name="maxContainerCount">Maximum container count to parse on container list</param>
        private CmdParser(Model model, IHadoopConnector connection, int maxContainerCount = 32)
        {
            Model = model;
            Connection = connection;
            MaxContainerCount = maxContainerCount;
        }

        /// <summary>
        /// Creates a new <see cref="CmdParser"/> instance, saves and returns it if a <see cref="CmdConnector"/> can be set
        /// </summary>
        /// <returns>Null if <see cref="CmdConnector"/> is not set, otherwise the instance</returns>
        internal static CmdParser CreateInstance()
        {
            return CreateInstance(CmdConnector.Instance);
        }

        /// <summary>
        /// Creates a new <see cref="CmdParser"/> instance using the given <see cref="IHadoopConnector"/>,
        /// saves and returns it
        /// </summary>
        /// <param name="connector">The <see cref="IHadoopConnector"/> to use</param>
        /// <returns>Null if <see cref="CmdConnector"/> is not set, otherwise the instance</returns>
        internal static CmdParser CreateInstance(IHadoopConnector connector)
        {
            if(connector == null)
                return null;

            var model = Model.Instance;
            _Instance = new CmdParser(model, connector);

            return _Instance;
        }

        #endregion

        #region IHadoopParser

        /// <summary>
        /// Gets and parses the <see cref="YarnApp"/> list for the given <see cref="EAppState"/>s.
        /// </summary>
        /// <param name="states">The states</param>
        /// <returns>The applications</returns>
        public IApplicationResult[] ParseAppList(EAppState states = EAppState.None)
        {
            var appStates = DriverUtilities.ConcatStates(states);

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
                    State = DriverUtilities.ParseAppState(appMatches[5].Groups[1].Value),
                    FinalStatus = DriverUtilities.ParseFinalStatus(appMatches[6].Groups[1].Value),
                    Progess = DriverUtilities.ParseInt(appMatches[7].Groups[1].Value),
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
        public IAppAttemptResult[] ParseAppAttemptList(string appId)
        {
            var fullResult = Connection.GetYarnAppAttemptList(appId);

            var attemptList = new List<AppAttemptResult>();
            var resLines = _LineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var attemptMatches = _GenericListRegex.Matches(resLine);
                if(attemptMatches.Count != 4 || !attemptMatches[0].Groups[1].Value.StartsWith("appattempt"))
                    continue;

                var attempt = new AppAttemptResult
                {
                    AttemptId = attemptMatches[0].Groups[1].Value,
                    State = DriverUtilities.ParseAppState(attemptMatches[1].Groups[1].Value),
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
        public IContainerResult[] ParseContainerList(string attemptId)
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
                    StartTime = DriverUtilities.ParseJavaTimestamp(containerMatches[1].Groups[1].Value, HadoopDateFormat),
                    FinishTime = DriverUtilities.ParseJavaTimestamp(containerMatches[2].Groups[1].Value, HadoopDateFormat),
                    State = DriverUtilities.ParseContainerState(containerMatches[3].Groups[1].Value),
                    Host = DriverUtilities.ParseNode(containerMatches[4].Groups[1].Value, Model),
                    LogUrl = containerMatches[6].Groups[1].Value,
                };

                containerList.Add(container);

                if(containerList.Count >= MaxContainerCount)
                    break;
            }

            return containerList.ToArray();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnApp"/> details, returns null if error appears
        /// </summary>
        /// <param name="appId">The <see cref="YarnApp.AppId"/> from the app</param>
        /// <returns>The application details or null on errors</returns>
        public IApplicationResult ParseAppDetails(string appId)
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
                    StartTime = DriverUtilities.ParseJavaTimestamp(matches[5].Groups[2].Value),
                    FinishTime = DriverUtilities.ParseJavaTimestamp(matches[6].Groups[2].Value),
                    Progess = DriverUtilities.ParseInt(matches[7].Groups[2].Value),
                    State = DriverUtilities.ParseAppState(matches[8].Groups[2].Value),
                    FinalStatus = DriverUtilities.ParseFinalStatus(matches[9].Groups[2].Value),
                    TrackingUrl = matches[10].Groups[2].Value,
                    AmHost = DriverUtilities.ParseNode(matches[12].Groups[2].Value, Model),
                    MbSeconds = DriverUtilities.ParseInt(resourcesMatches[0].Value),
                    VcoreSeconds = DriverUtilities.ParseInt(resourcesMatches[1].Value),
                    Diagnostics = matches[14].Groups[2].Value.Replace('\n', ' '),
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
        public IAppAttemptResult ParseAppAttemptDetails(string attemptId)
        {
            var fullResult = Connection.GetYarnAppAttemptDetails(attemptId);

            var matches = _GenericDetailsRegex.Matches(fullResult);

            if(matches.Count == 7)
            {
                var attempt = new AppAttemptResult
                {
                    AttemptId = matches[0].Groups[2].Value,
                    State = DriverUtilities.ParseAppState(matches[1].Groups[2].Value),
                    AmContainerId = matches[2].Groups[2].Value,
                    TrackingUrl = matches[3].Groups[2].Value,
                    AmHost = DriverUtilities.ParseNode(matches[5].Groups[2].Value, Model),
                    Diagnostics = matches[6].Groups[2].Value.Replace('\n', ' '),
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
        public IContainerResult ParseContainerDetails(string containerId)
        {
            var fullResult = Connection.GetYarnAppContainerDetails(containerId);

            var matches = _GenericDetailsRegex.Matches(fullResult);

            if(matches.Count == 8)
            {
                var container = new ContainerResult
                {
                    ContainerId = matches[0].Groups[2].Value,
                    StartTime = DriverUtilities.ParseJavaTimestamp(matches[1].Groups[2].Value),
                    FinishTime = DriverUtilities.ParseJavaTimestamp(matches[2].Groups[2].Value),
                    State = DriverUtilities.ParseContainerState(matches[3].Groups[2].Value),
                    LogUrl = matches[4].Groups[2].Value,
                    Host = DriverUtilities.ParseNode(matches[5].Groups[2].Value, Model),
                    Diagnostics = matches[7].Groups[2].Value.Replace('\n', ' '),
                };

                return container;
            }

            return null;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnNode"/> list for the cluster
        /// </summary>
        /// <returns>All nodes in the cluster</returns>
        public INodeResult[] ParseNodeList()
        {
            var fullResult = Connection.GetYarnNodeList();

            var nodeList = new List<NodeResult>();
            var resLines = _LineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var nodeMatches = _GenericListRegex.Matches(resLine);
                if(nodeMatches.Count != 4 || !nodeMatches[0].Groups[1].Value.StartsWith(ModelSettings.NodeNamePrefix))
                    continue;

                var node = new NodeResult
                {
                    NodeId = nodeMatches[0].Groups[1].Value,
                    NodeState = DriverUtilities.ParseNodeState(nodeMatches[1].Groups[1].Value),
                    NodeHttpAdd = nodeMatches[2].Groups[1].Value,
                    RunningContainerCount = DriverUtilities.ParseInt(nodeMatches[3].Groups[1].Value),
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
        public INodeResult ParseNodeDetails(string nodeId)
        {
            var fullResult = Connection.GetYarnNodeDetails(nodeId);

            var matches = _GenericDetailsRegex.Matches(fullResult);

            if(matches.Count == 12)
            {
                var node = new NodeResult
                {
                    NodeId = matches[0].Groups[2].Value,
                    NodeState = DriverUtilities.ParseNodeState(matches[2].Groups[2].Value),
                    NodeHttpAdd = matches[3].Groups[2].Value,
                    RunningContainerCount = DriverUtilities.ParseInt(matches[6].Groups[2].Value),
                    MemoryUsed = DriverUtilities.ParseInt(matches[7].Groups[2].Value),
                    MemoryCapacity = DriverUtilities.ParseInt(matches[8].Groups[2].Value),
                    CpuUsed = DriverUtilities.ParseInt(matches[9].Groups[2].Value),
                    CpuCapacity = DriverUtilities.ParseInt(matches[10].Groups[2].Value),
                };

                return node;
            }

            return null;
        }

        /// <summary>
        /// Gets and parses the current MARP value from the controllers scheduler.
        /// If value cannot be getted by the scheduler, the default value 0.0 will be returned.
        /// </summary>
        /// <returns>The current MARP value or the default value</returns>
        public double ParseMarpValue()
        {
            var valueStr = Connection.GetMarpValue();
            double result;
            if(Double.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;
            return result;
        }

        #endregion
    }
}

