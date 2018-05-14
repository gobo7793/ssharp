#region License
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
using System.Linq;
using Newtonsoft.Json;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Connector;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.DataClasses;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Parser
{
    public class RestParser : IHadoopParser
    {
        #region Properties and Constants

        private static log4net.ILog Logger { get; } = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static RestParser _Instance;

        /// <summary>
        /// <see cref="RestParser"/> instance
        /// </summary>
        public static RestParser Instance => _Instance ?? CreateInstance();

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
        private RestParser(Model model, IHadoopConnector connection)
        {
            Model = model;
            Connection = connection;
        }

        /// <summary>
        /// Creates a new <see cref="RestParser"/> instance, saves and returns it if a <see cref="RestConnector"/> can be set
        /// </summary>
        /// <returns>Null if <see cref="RestConnector"/> is not set, otherwise the instance</returns>
        internal static RestParser CreateInstance()
        {
            return CreateInstance(RestConnector.Instance);
        }

        /// <summary>
        /// Creates a new <see cref="RestParser"/> instance using the given <see cref="IHadoopConnector"/>,
        /// saves and returns it
        /// </summary>
        /// <param name="connector">The <see cref="IHadoopConnector"/> to use</param>
        /// <returns>Null if <see cref="RestParser"/> is not set, otherwise the instance</returns>
        internal static RestParser CreateInstance(IHadoopConnector connector)
        {
            if(connector == null)
                return null;

            var model = Model.Instance;
            _Instance = new RestParser(model, connector);

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

            Logger.Debug($"Parse app list '{appStates}'");

            var fullResult = Connection.GetYarnApplicationList(appStates);
            var appRes = JsonConvert.DeserializeObject<ApplicationListJsonResult>(fullResult);

            // convert AM Hosts
            var apps = appRes.Collection?.List;
            if(apps != null)
            {
                foreach(var app in apps)
                {
                    Logger.Debug($"Parsing app '{app.AppId}'");
                    if(!String.IsNullOrWhiteSpace(app.AmHostHttpAddress)) // if app is in preparing states
                        app.AmHost = DriverUtilities.ParseNode(app.AmHostHttpAddress, Model);
                }
            }

            return apps;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppAttempt"/> list for the given <see cref="YarnApp.AppId"/>
        /// </summary>
        /// <param name="appId">The app id</param>
        /// <returns>The attempts</returns>
        public IAppAttemptResult[] ParseAppAttemptList(string appId)
        {
            Logger.Debug($"Parse attempt list for app '{appId}'");

            var fullResult = Connection.GetYarnAppAttemptList(appId);
            var tlResult = Connection.GetYarnAppAttemptListTl(appId);
            var attemptRes = JsonConvert.DeserializeObject<JsonAppAttemptListResult>(fullResult);

            JsonAppAttemptResultCollection tlAttempts = null;
            if(!String.IsNullOrWhiteSpace(tlResult))
                tlAttempts = JsonConvert.DeserializeObject<JsonAppAttemptResultCollection>(tlResult);

            var attempts = attemptRes.Collection.List;
            foreach(var attempt in attempts)
            {
                Logger.Debug($"Processing attempt '{attempt.AttemptId}'");
                attempt.AmHost = DriverUtilities.ParseNode(attempt.AmHostId, Model);

                // get more info from timeline server
                string attemptId;
                var parsedId = DriverUtilities.ParseInt(attempt.AttemptId);
                if(parsedId != 0)
                {
                    attemptId = DriverUtilities.ConvertId(appId, parsedId, EConvertType.Attempt);
                    attempt.AttemptId = attemptId;
                }
                else
                    attemptId = attempt.AttemptId;

                var tlAttempt = tlAttempts?.List?.FirstOrDefault(a => a.AttemptId == attemptId);
                if(tlAttempt != null)
                {
                    //attempt.AttemptId = tlAttempt.AttemptId;
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
        public IContainerResult[] ParseContainerList(string attemptId)
        {
            Logger.Debug($"Parse container list for attempt '{attemptId}'");

            var containerList = new List<ContainerResult>();

            var baseContainerId = DriverUtilities.ConvertId(attemptId, EConvertType.Container);
            var tlContainerResult = Connection.GetYarnAppContainerListTl(attemptId);

            // Get from RM
            foreach(var node in Model.Nodes)
            {
                var containerResult = Connection.GetYarnAppContainerList(node.HttpUrl);
                var containers = JsonConvert.DeserializeObject<JsonContainerListResult>(containerResult);
                if(containers?.Collection?.List?.Length > 0)
                    foreach(var con in containers.Collection.List)
                    {
                        if(con.ContainerId.StartsWith(baseContainerId))
                        {
                            con.Host = node;
                            containerList.Add(con);
                        }
                    }
            }

            // Add TL infos
            if(!String.IsNullOrWhiteSpace(tlContainerResult))
            {
                var tlContainers = JsonConvert.DeserializeObject<JsonContainerResultCollection>(tlContainerResult);

                //if(containerList.Count == 0 && tlContainers?.List?.Length > 0)
                //    return tlContainers.List; // if only TL
                if(tlContainers?.List == null || tlContainers.List.Length == 0)
                    return containerList.ToArray(); // if nothing in TL

                // if booth lists
                var originalContainers = containerList.ToDictionary(c => c.ContainerId);
                foreach(var tlContainer in tlContainers.List)
                {
                    Logger.Debug($"Processing tl container '{tlContainer.ContainerId}'");

                    // merge tl data to rm data
                    if(originalContainers.ContainsKey(tlContainer.ContainerId))
                    {
                        originalContainers[tlContainer.ContainerId].Priority = tlContainer.Priority;
                        originalContainers[tlContainer.ContainerId].StartTime = tlContainer.StartTime;
                        originalContainers[tlContainer.ContainerId].FinishTime = tlContainer.FinishTime;
                    }
                    //else // copy tl containers
                    //{
                    //    tlContainer.Host = DriverUtilities.ParseNode(tlContainer.HostId, Model);
                    //    originalContainers[tlContainer.ContainerId] = tlContainer;
                    //}
                }
                return originalContainers.Values.ToArray();
            }

            return containerList.ToArray();
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnApp"/> details
        /// </summary>
        /// <param name="appId">The <see cref="YarnApp.AppId"/> from the app</param>
        /// <returns>The application details</returns>
        public IApplicationResult ParseAppDetails(string appId)
        {
            var fullResult = Connection.GetYarnApplicationDetails(appId);
            var app = JsonConvert.DeserializeObject<ApplicationDetailsJsonResult>(fullResult).App;

            // convert AM Hosts
            app.AmHost = DriverUtilities.ParseNode(app.AmHostHttpAddress, Model);

            return app;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppAttempt"/> details
        /// </summary>
        /// <param name="attemptId">The <see cref="YarnAppAttempt.AttemptId"/> from the attempt</param>
        /// <returns>The attempt details</returns>
        public IAppAttemptResult ParseAppAttemptDetails(string attemptId)
        {
            var appId = DriverUtilities.ConvertId(attemptId, EConvertType.App);
            var attempts = ParseAppAttemptList(appId);
            return attempts.FirstOrDefault(a => a.AttemptId == attemptId);

            //var appId = DriverUtilities.ConvertId(attemptId, EConvertType.App);

            //var allAttemptsRes = Connection.GetYarnAppAttemptList(appId);
            //var tlDetailsRes = Connection.GetYarnAppAttemptDetailsTl(attemptId);
            //var allAttempts = JsonConvert.DeserializeObject<JsonAppAttemptListResult>(allAttemptsRes);

            //var attempt = allAttempts.Collection.List.FirstOrDefault(a => attemptId.EndsWith(a.AttemptId));
            //if(attempt != null)
            //{
            //    attempt.AttemptId = attemptId;
            //    attempt.AmHost = DriverUtilities.ParseNode(attempt.AmHostId, Model);
            //    if(!String.IsNullOrWhiteSpace(tlDetailsRes))
            //    {
            //        var tlDetails = JsonConvert.DeserializeObject<AppAttemptResult>(tlDetailsRes);
            //        attempt.TrackingUrl = tlDetails.TrackingUrl;
            //        attempt.Diagnostics = tlDetails.Diagnostics;
            //        attempt.State = tlDetails.State;
            //    }
            //}

            //return attempt;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnAppContainer"/> details from the given <see cref="YarnNode"/>
        /// </summary>
        /// <param name="containerId">The <see cref="YarnAppContainer.ContainerId"/> from the container</param>
        /// <returns>The container details</returns>
        public IContainerResult ParseContainerDetails(string containerId)
        {
            var attemptId = DriverUtilities.ConvertId(containerId, EConvertType.Attempt);
            var allContainers = ParseContainerList(attemptId);
            return allContainers.FirstOrDefault(c => c.ContainerId == containerId);

            //var containerResult = Connection.GetYarnAppContainerDetails(containerId, node.HttpUrl);
            //var tlContainerResult = Connection.GetYarnAppContainerDetailsTl(containerId);

            //ContainerResult container = null;
            //if(!String.IsNullOrWhiteSpace(containerResult))
            //{
            //    var containerRes = JsonConvert.DeserializeObject<ContainerDetailsJsonResult>(containerResult);
            //    if(containerRes != null)
            //        container = containerRes.Container;
            //}

            //if(!String.IsNullOrWhiteSpace(tlContainerResult))
            //{
            //    var tlContainerRes = JsonConvert.DeserializeObject<ContainerResult>(tlContainerResult);
            //    if(container == null)
            //        container = tlContainerRes;
            //    else
            //    {
            //        container.Priority = tlContainerRes.Priority;
            //        container.StartTime = tlContainerRes.StartTime;
            //        container.FinishTime = tlContainerRes.FinishTime;
            //    }
            //}

            //if(container != null)
            //    container.Host = ParserUtilities.ParseNode(container.HostId, Model);
            //return container;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnNode"/> list for the cluster
        /// </summary>
        /// <returns>All nodes in the cluster</returns>
        public INodeResult[] ParseNodeList()
        {
            Logger.Debug("Parsing node list");

            var fullResult = Connection.GetYarnNodeList();
            var nodeRes = JsonConvert.DeserializeObject<NodeListJsonResult>(fullResult);

            return nodeRes.Collection.List;
        }

        /// <summary>
        /// Gets and parses the <see cref="YarnNode"/> details
        /// </summary>
        /// <param name="nodeId">The <see cref="YarnNode.NodeId"/> from the node</param>
        /// <returns>The node details</returns>
        public INodeResult ParseNodeDetails(string nodeId)
        {
            Logger.Debug($"Parsing node '{nodeId}'");

            var fullResult = Connection.GetYarnNodeDetails(nodeId);
            var node = JsonConvert.DeserializeObject<NodeDetailsJsonResult>(fullResult).Node;

            return node;
        }

        #endregion
    }
}