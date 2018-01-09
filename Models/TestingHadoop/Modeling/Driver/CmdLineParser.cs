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
using System.Globalization;
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
        private static readonly Regex _genericListRegex = new Regex(@"\s*([^\t]+)");

        /// <summary>
        /// Generic regex for details
        /// </summary>
        /// <remarks>
        /// Group 1: the property name without leading or trailing whitespaces
        /// Group 2: the property value without leading or trailing whitespaces
        /// </remarks>
        private static readonly Regex _genericDetailsRegex = new Regex(@"^\t(.+)\s:\s(.*)$", RegexOptions.Multiline);

        /// <summary>
        /// Line splitter regex
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

        /// <summary>
        /// Parses the <see cref="EAppState"/> or returns the default value <see cref="EAppState.UNKNOWN"/>
        /// </summary>
        /// <param name="state">The state to parse</param>
        /// <returns>The parsed <see cref="EAppState"/></returns>
        private EAppState ParseState(string state)
        {
            EAppState parsedState;
            Enum.TryParse(state, true, out parsedState);
            return parsedState;
        }

        /// <summary>
        /// Parses the integer or returns the default value 0
        /// </summary>
        /// <param name="value">The value to parse</param>
        /// <returns>The parsed integer</returns>
        private int ParseInt(string value)
        {
            int val;
            Int32.TryParse(value, out val);
            return val;
        }

        /// <summary>
        /// Parses the timestamp with the given format to <see cref="DateTime"/>
        /// or returns the default value <see cref="DateTime.MinValue"/>
        /// </summary>
        /// <param name="value">The value to parse</param>
        /// <param name="format">The time format for parsing or null if convert vom Java Time Millisec</param>
        /// <param name="culture">The <see cref="CultureInfo"/> for parsing, default en-US</param>
        /// <returns>The parsed <see cref="DateTime"/></returns>
        public DateTime ParseTimestamp(string value, string format, CultureInfo culture = null)
        {
            culture = culture ?? new CultureInfo("en-US");
            if(format != null)
            {
                DateTime time;
                DateTime.TryParseExact(value, format, culture, DateTimeStyles.AssumeUniversal, out time);
                return time;
            }

            var javaMillis = ParseInt(value);
            var ticks = javaMillis * TimeSpan.TicksPerMillisecond;
            var javaBaseTime = new DateTime(1970, 1, 1);
            var javaTimeSpan = TimeSpan.FromTicks(ticks);
            return javaBaseTime.Add(javaTimeSpan);
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
            var appStates = String.Join(",", states);

            var fullResult = Connection.GetYarnApplicationList(appStates);

            var appList = new List<ApplicationListResult>();
            var resLines = _lineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var appMatches = _genericListRegex.Matches(resLine);
                if(appMatches.Count != 9 || !appMatches[0].Groups[1].Value.StartsWith("application"))
                    continue;

                var state = ParseState(appMatches[5].Groups[1].Value);
                var progress = ParseInt(appMatches[7].Groups[1].Value.Substring(0, appMatches[7].Groups[1].Value.Length - 1));

                var app = new ApplicationListResult(appMatches[0].Groups[1].Value, appMatches[1].Groups[1].Value,
                    appMatches[2].Groups[1].Value, state, appMatches[6].Groups[1].Value, progress, appMatches[8].Groups[1].Value);

                appList.Add(app);
            }

            return appList.ToArray();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppAttempt"/> list for the given <see cref="YarnApp.AppId"/>
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The attempts</returns>
        public ApplicationAttemptListResult[] ParseAppAttemptList(string appId)
        {
            var fullResult = Connection.GetYarnAppAttemptList(appId);

            var attemptList = new List<ApplicationAttemptListResult>();
            var resLines = _lineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var attemptMatches = _genericListRegex.Matches(resLine);
                if(attemptMatches.Count != 4 || !attemptMatches[0].Groups[1].Value.StartsWith("appattempt"))
                    continue;

                var state = ParseState(attemptMatches[1].Groups[1].Value);

                var attempt = new ApplicationAttemptListResult(attemptMatches[0].Groups[1].Value, state, attemptMatches[2].Groups[1].Value,
                    attemptMatches[3].Groups[1].Value);

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
            var fullResult = Connection.GetYarnAppContainerList(attemptId);

            var containerList = new List<ContainerListResult>();
            var resLines = _lineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var containerMatches = _genericListRegex.Matches(resLine);
                if(containerMatches.Count != 7 || !containerMatches[0].Groups[1].Value.StartsWith("container"))
                    continue;

                var startTime = ParseTimestamp(containerMatches[1].Groups[1].Value, HadoopDateFormat);
                var finishTime = ParseTimestamp(containerMatches[2].Groups[1].Value, HadoopDateFormat);
                var state = ParseState(containerMatches[3].Groups[1].Value);

                var node = Model.Nodes[containerMatches[4].Groups[1].Value.Split(':')[0]];

                var container = new ContainerListResult(containerMatches[0].Groups[1].Value, startTime, finishTime, state, node,
                    containerMatches[6].Groups[1].Value);

                containerList.Add(container);
            }

            return containerList.ToArray();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnApp"/> details, returns null if error appears
        /// </summary>
        /// <param name="appId">The <see cref="YarnApp.AppId"/> from the app</param>
        /// <returns>The application details or null on errors</returns>
        public ApplicationDetailsResult ParseAppDetails(string appId)
        {
            var fullResult = Connection.GetYarnApplicationDetails(appId);

            var matches = _genericDetailsRegex.Matches(fullResult);

            if(matches.Count == 15)
            {
                var state = ParseState(matches[8].Groups[2].Value);
                var progress = ParseInt(matches[7].Groups[2].Value);
                var startTime = ParseTimestamp(matches[5].Groups[2].Value, null);
                var finishTime = ParseTimestamp(matches[6].Groups[2].Value, null);
                var node = Model.Nodes[matches[12].Groups[2].Value];

                var resMatches = Regex.Matches(matches[13].Groups[2].Value, @"\d+");
                var mbSec = ParseInt(resMatches[0].Value);
                var vcSec = ParseInt(resMatches[1].Value);

                var app = new ApplicationDetailsResult(matches[0].Groups[2].Value, matches[1].Groups[2].Value, matches[2].Groups[2].Value,
                    state, matches[9].Groups[2].Value, progress, matches[10].Groups[2].Value, startTime, finishTime, node, mbSec, vcSec);

                return app;
            }

            return null;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppAttempt"/> details, returns null if error appears
        /// </summary>
        /// <param name="attemptId">The <see cref="YarnAppAttempt.AttemptId"/> from the attempt</param>
        /// <returns>The attempt details or null on errors</returns>
        public ApplicationAttemptDetailsResult ParseAppAttemptDetails(string attemptId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppContainer"/> details, returns null if error appears
        /// </summary>
        /// <param name="containerId">The <see cref="YarnAppContainer.ContainerId"/> from the container</param>
        /// <returns>The container details or null on errors</returns>
        public ContainerListResult ParseContainerDetails(string containerId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnNode"/> list for the cluster
        /// </summary>
        /// <returns>All nodes in the cluster</returns>
        public NodeListResult[] ParseNodeList()
        {
            var fullResult = Connection.GetYarnNodeList();

            var nodeList = new List<NodeListResult>();
            var resLines = _lineSplitterRegex.Split(fullResult);

            foreach(var resLine in resLines)
            {
                var nodeMatches = _genericListRegex.Matches(resLine);
                if(nodeMatches.Count != 4 || !nodeMatches[0].Groups[1].Value.StartsWith(Model.NodeNamePrefix))
                    continue;

                int containerCount;
                Int32.TryParse(nodeMatches[3].Groups[1].Value, out containerCount);

                var app = new NodeListResult(nodeMatches[0].Groups[1].Value, nodeMatches[1].Groups[1].Value,
                    nodeMatches[2].Groups[1].Value, containerCount);

                nodeList.Add(app);
            }

            return nodeList.ToArray();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnNode"/> details, returns null if error appears
        /// </summary>
        /// <param name="nodeId">The <see cref="YarnNode.NodeId"/> from the node</param>
        /// <returns>The node details or null on errors</returns>
        public NodeDetailsResult ParseNodeDetails(string nodeId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}