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

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// YARN Controller
    /// </summary>
    public class YarnController : YarnHost
    {
        protected override string HttpPort => "8088";

        /// <summary>
        /// HTTP URL of the timeline server
        /// </summary>
        public string TimelineHttpUrl => $"http://{Name}:8188";

        /// <summary>
        /// Connected <see cref="YarnNode" />s
        /// </summary>
        public List<YarnNode> ConnectedNodes { get; } = new List<YarnNode>();

        /// <summary>
        /// Connected <see cref="Client" />
        /// </summary>
        public Client ConnectedClient { get; set; }

        /// <summary>
        /// The executed <see cref="YarnApp"/>s
        /// </summary>
        public List<YarnApp> Apps { get; set; } = new List<YarnApp>();

        /// <summary>
        /// Gets all apps executed on the cluster
        /// </summary>
        public void GetApps()
        {
            var apps = Parser.ParseAppList(EAppState.ALL);
            if(apps.Length > 0)
            {
                foreach(var con in apps)
                {
                    if(Apps.All(c => c.AppId != con.AppId))
                    {
                        var usingApp = Apps.FirstOrDefault(c => String.IsNullOrWhiteSpace(c.AppId));
                        if(usingApp == null)
                            throw new OutOfMemoryException("No more applications available!" +
                                                           " Try to initialize more application space.");
                        usingApp.AppId = con.AppId;
                    }
                }
            }
        }

        public override void Update()
        {
            GetApps();
        }
    }
}