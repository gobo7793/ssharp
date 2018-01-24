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
            var tlResult = Connection.GetYarnAppAttemptListTl(appId);
            var attemptRes = JsonConvert.DeserializeObject<JsonAppAttemptListResult>(fullResult);

            JsonAppAttemptResultCollection tlAttempts = null;
            if(!String.IsNullOrWhiteSpace(tlResult))
                tlAttempts = JsonConvert.DeserializeObject<JsonAppAttemptResultCollection>(tlResult);

            var attempts = attemptRes.Collection.List;
            foreach(var attempt in attempts)
            {
                attempt.AmHost = ParserUtilities.ParseNode(attempt.AmHostId, Model);

                // get more info from timeline server
                string attemptId = String.Empty;
                var parsedId = ParserUtilities.ParseInt(attempt.AttemptId);
                if(parsedId != 0)
                    attemptId = ParserUtilities.BuildAttemptIdFromApp(appId, parsedId);
                else attemptId = attempt.AttemptId;

                var tlAttempt = tlAttempts?.List?.FirstOrDefault(a => a.AttemptId == attemptId);
                if(tlAttempt != null)
                {
                    attempt.AttemptId = tlAttempt.AttemptId;
                    attempt.State = tlAttempt.State;
                    attempt.TrackingUrl = tlAttempt.TrackingUrl;
                    attempt.Diagnostics = tlAttempt.Diagnostics;
                }
            }

            return attempts;
        }

        /// <summary>
        /// Gets and parses the current running <see cref="YarnAppContainer"/> list for the given <see cref="YarnAppAttempt.AttemptId"/>.
        /// Timeline data overrides RM data!
        /// </summary>
        /// <param name="attemptId">The attempt id</param>
        /// <returns>The running containers</returns>
        public ContainerResult[] ParseContainerList(string attemptId)
        {
            var containerList = new List<ContainerResult>();

            var baseContainerId = ParserUtilities.BuildBaseContainerIdFromAttempt(attemptId);
            var tlContainerResult = Connection.GetYarnAppContainerListTl(attemptId);

            // Get from RM
            foreach(var node in Model.Nodes)
            {
                var containerResult = Connection.GetYarnAppContainerList(node.Value.HttpUrl);
                var containers = JsonConvert.DeserializeObject<JsonContainerListResult>(containerResult);
                if(containers?.Collection?.List?.Length > 0)
                    foreach(var con in containers.Collection.List)
                    {
                        if(con.ContainerId.StartsWith(baseContainerId))
                        {
                            con.Host = node.Value;
                            containerList.Add(con);
                        }
                    }
            }

            // Add TL infos
            if(!String.IsNullOrWhiteSpace(tlContainerResult))
            {
                var tlContainers = JsonConvert.DeserializeObject<JsonContainerResultCollection>(tlContainerResult);

                if(containerList.Count == 0 && tlContainers?.List?.Length > 0)
                    return tlContainers.List; // if only TL
                if(tlContainers?.List == null || tlContainers.List.Length == 0)
                    return containerList.ToArray(); // if nothing in TL

                // if booth
                var originalContainers = containerList.ToDictionary(c => c.ContainerId);
                foreach(var tlContainer in tlContainers.List)
                {
                    // merge tl data to rm data
                    if(originalContainers.ContainsKey(tlContainer.ContainerId))
                    {
                        originalContainers[tlContainer.ContainerId].Priority = tlContainer.Priority;
                        originalContainers[tlContainer.ContainerId].StartTime = tlContainer.StartTime;
                        originalContainers[tlContainer.ContainerId].FinishTime = tlContainer.FinishTime;
                    }
                    else // copy else
                    {
                        tlContainer.Host = ParserUtilities.ParseNode(tlContainer.HostId, Model);
                        originalContainers[tlContainer.ContainerId] = tlContainer;
                    }
                }
                return originalContainers.Values.ToArray();
            }

            return containerList.ToArray();
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
            var appId = ParserUtilities.BuildAppIdFromAttempt(attemptId);

            var allAttemptsRes = Connection.GetYarnAppAttemptList(appId);
            var tlDetailsRes = Connection.GetYarnAppAttemptDetailsTl(attemptId);
            var allAttempts = JsonConvert.DeserializeObject<JsonAppAttemptListResult>(allAttemptsRes);

            var attempt = allAttempts.Collection.List.FirstOrDefault(a => attemptId.EndsWith(a.AttemptId));
            if(attempt != null)
            {
                attempt.AttemptId = attemptId;
                attempt.AmHost = ParserUtilities.ParseNode(attempt.AmHostId, Model);
                if(!String.IsNullOrWhiteSpace(tlDetailsRes))
                {
                    var tlDetails = JsonConvert.DeserializeObject<ApplicationAttemptResult>(tlDetailsRes);
                    attempt.TrackingUrl = tlDetails.TrackingUrl;
                    attempt.Diagnostics = tlDetails.Diagnostics;
                    attempt.State = tlDetails.State;
                }
            }

            return attempt;
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