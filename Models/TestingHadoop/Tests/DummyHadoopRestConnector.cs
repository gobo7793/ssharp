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
using System.IO;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    public class DummyHadoopRestConnector : IHadoopConnector
    {
        private string _JsonFilesPath;

        /// <summary>
        /// Creates a new dummy connector for Hadoop YARN REST API
        /// and uses the json files on the setted directory
        /// </summary>
        public DummyHadoopRestConnector()
        {
            var relativePath = @"..\..\..\Models\TestingHadoop\Tests\RestTestCases";
            _JsonFilesPath = Path.GetFullPath(Environment.CurrentDirectory + relativePath);

            Console.WriteLine($@"Using ""{_JsonFilesPath}"" to search json test case sources");
        }

        /// <summary>
        /// Reads the file in <see cref="_JsonFilesPath"/>
        /// </summary>
        /// <param name="filename">The filename</param>
        /// <returns>File content</returns>
        private string ReadFile(string filename) => File.ReadAllText($@"{_JsonFilesPath}\{filename}");

        public string GetYarnApplicationList(string states) => ReadFile("apps.json");

        public string GetYarnAppAttemptList(string appId) => ReadFile("appattempts.json");

        public string GetYarnAppAttemptListTl(string appId) => ReadFile("tlappattempts.json");

        public string GetYarnAppContainerList(string id)
        {
            switch(id)
            {
                case "http://compute-1:8042":
                    return ReadFile("node1runningContainers.json");
                case "http://compute-2:8042":
                    return ReadFile("node2runningContainers.json");
                case "http://compute-3:8042":
                    return ReadFile("node3runningContainers.json");
                case "http://compute-4:8042":
                    return ReadFile("node4runningContainers.json");
            }
            return String.Empty;
        }

        public string GetYarnAppContainerListTl(string attemptId) => ReadFile("tlrunningContainers.json");

        public string GetYarnApplicationDetails(string appId) => ReadFile("appsDetails.json");

        public string GetYarnAppAttemptDetails(string attemptId)
        {
            throw new PlatformNotSupportedException();
        }

        public string GetYarnAppAttemptDetailsTl(string attemptId) => ReadFile("tlappattemptsdetails.json");

        public string GetYarnAppContainerDetails(string containerId) => ReadFile("node4containersDetails.json");

        public string GetYarnAppContainerDetailsTl(string containerId) => ReadFile("tlcontainersDetails.json");

        public string GetYarnNodeList() => ReadFile("nodes.json");

        /// <summary>
        /// If given "dead": Returns the dead, else the running details
        /// </summary>
        /// <param name="nodeId">The given arg</param>
        /// <returns>Dead or running details</returns>
        public string GetYarnNodeDetails(string nodeId)
        {
            if(nodeId == "dead")
                return ReadFile("nodeDead.json");
            return ReadFile("nodesDetails.json");
        }

        public bool StartNode(string nodeName)
        {
            throw new NotImplementedException();
        }

        public bool StopNode(string nodeName)
        {
            throw new NotImplementedException();
        }

        public bool StartNodeNetConnection(string nodeName)
        {
            throw new NotImplementedException();
        }

        public bool StopNodeNetConnection(string nodeName)
        {
            throw new NotImplementedException();
        }

        public bool KillApplication(string appId)
        {
            throw new NotImplementedException();
        }
    }
}