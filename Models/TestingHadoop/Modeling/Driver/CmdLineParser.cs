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
        private const string GenericListRegexPattern = @"([^\s])";

        /// <summary>
        /// Line splitter regex pattern
        /// </summary>
        private const string LineSplitterRegexPattern = @"\r\n|\r|\n";

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

            var appCountRegex = new Regex(@"Total number of applications[^\d]*(\d+)");
            var listRegex = new Regex(GenericListRegexPattern);

            int appCount = -1;
            ApplicationListResult[] appList = new ApplicationListResult[0];
            var resLines = Regex.Split(fullResult, LineSplitterRegexPattern);

            foreach(var resLine in resLines)
            {
                if(appList.Length < 1)
                {
                    var countRegexMatch = appCountRegex.Match(resLine);
                    if(countRegexMatch.Success)
                    {
                        appCount = Int32.Parse(countRegexMatch.Groups[1].Value);
                        appList = new ApplicationListResult[appCount];
                    }
                }
            }

            return appList;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppAttempt"/> list for the given <see cref="YarnApp.AppId"/>
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The attempts</returns>
        public ApplicationAttemptListResult[] ParseAttemptAttemptList(string appId)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets and parses the current running <see cref="YarnAppContainer"/> list for the given <see cref="YarnAppAttempt.AttemptId"/>
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The running containers</returns>
        public ContainerListResult[] ParseContainerList(string attemptId)
        {
            throw new System.NotImplementedException();
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