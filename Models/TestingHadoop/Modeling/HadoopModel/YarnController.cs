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
using System.Linq;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// YARN Controller
    /// </summary>
    public class YarnController : YarnHost
    {

        #region Properties

        /// <summary>
        /// HTTP URL of the timeline server
        /// </summary>
        public string TimelineHttpUrl { get; private set; }

        /// <summary>
        /// Connected <see cref="Client" />
        /// </summary>
        public List<Client> ConnectedClients { get; private set; }

        /// <summary>
        /// Connected <see cref="YarnNode" />s
        /// </summary>
        public List<YarnNode> ConnectedNodes { get; private set; }

        /// <summary>
        /// The executed <see cref="YarnApp"/>s on the cluster
        /// </summary>
        public List<YarnApp> Apps { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new empty <see cref="YarnController"/>
        /// </summary>
        public YarnController()
        {
            InitYarnController();
        }

        /// <summary>
        /// Initialize a new <see cref="YarnController"/>
        /// </summary>
        /// <param name="name">Name of the Host</param>
        public YarnController(string name) : base(name, 8088)
        {
            InitYarnController();
            TimelineHttpUrl = $"http://{Name}:8188";
        }

        private void InitYarnController()
        {
            ConnectedClients = new List<Client>();
            ConnectedNodes = new List<YarnNode>();
            Apps = new List<YarnApp>();
        }

        #endregion

        #region General methods

        public override void Update()
        {
            MonitorNodes();
            MonitorApps();
        }

        #endregion

        #region Node related methods

        /// <summary>
        /// Gets all node informations
        /// </summary>
        public void MonitorNodes()
        {
            var parsedNodes = Parser.ParseNodeList();
            foreach(var parsed in parsedNodes)
            {
                var node = ConnectedNodes.FirstOrDefault(n => n.NodeId == parsed.NodeId);
                if(node == null)
                    continue;

                node.SetStatus(parsed);
                node.IsSelfMonitoring = false;
            }
        }

        #endregion

        #region App related Methods

        /// <summary>
        /// Gets all apps executed on the cluster and their informations
        /// </summary>
        public void MonitorApps()
        {
            var parsedApps = Parser.ParseAppList(EAppState.ALL);
            foreach(var parsed in parsedApps)
            {
                var app = Apps.FirstOrDefault(a => a.AppId == parsed.AppId);// ??
                //          Apps.FirstOrDefault(a => String.IsNullOrWhiteSpace(a.AppId));
                //if(app == null)
                //    throw new OutOfMemoryException("No more applications available! Try to initialize more applications.");

                if(app == null)
                    continue;

                app.SetStatus(parsed);
                app.IsSelfMonitoring = false;
                app.MonitorStatus();
            }
        }

        #endregion

    }
}