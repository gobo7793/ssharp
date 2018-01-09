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
using NUnit.Framework;
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;
using static SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel.EAppState;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    [TestFixture]
    public class CmdLineParserTest
    {
        private CmdLineParser _Parser;
        private Model _Model;
        private YarnNode _Node1;

        [SetUp]
        public void Setup()
        {
            _Node1 = new YarnNode
            {
                Name = "compute-1",
            };
            _Model = new Model();
            _Model.Nodes.Add(_Node1);

            _Parser = new CmdLineParser(_Model, new DummyHadoopConnector());
        }

        [Test]
        public void TestParseAppList()
        {
            var app1 = new ApplicationListResult("application_1515488762656_0001", "random-text-writer", "MAPREDUCE",
                FINISHED, "SUCCEEDED", 100, "http://controller:19888/jobhistory/job/job_1515488762656_0001");
            var app2 = new ApplicationListResult("application_1515488762656_0002", "word count", "MAPREDUCE",
                FINISHED, "SUCCEEDED", 100, "http://controller:19888/jobhistory/job/job_1515488762656_0002");
            var app3 = new ApplicationListResult("application_1515488762656_0003", "sorter", "MAPREDUCE",
                FINISHED, "SUCCEEDED", 100, "http://controller:19888/jobhistory/job/job_1515488762656_0003");

            var apps = _Parser.ParseAppList();

            Assert.AreEqual(3, apps.Length, "wrong parsed apps count");
            Assert.AreEqual(app1, apps[0], "app1 failed");
            Assert.AreEqual(app2, apps[1], "app2 failed");
            Assert.AreEqual(app3, apps[2], "app3 failed");
        }

        [Test]
        public void TestParseAppAttemptList()
        {
            var attempt1 = new ApplicationAttemptListResult("appattempt_1515488762656_0002_000001", FINISHED,
                "container_1515488762656_0002_01_000001", "http://controller:8088/proxy/application_1515488762656_0002/");

            var attempts = _Parser.ParseAppAttemptList("");

            Assert.AreEqual(1, attempts.Length, "wrong parsed attempts count");
            Assert.AreEqual(attempt1, attempts[0], "attempt1 failed");
        }

        [Test]
        public void TestParseContainerList()
        {
            var container1 = new ContainerListResult("container_1515488762656_0011_01_000001", new DateTime(2018, 1, 9, 10, 41, 14),
                DateTime.MinValue, RUNNING, _Node1, "http://compute-2:8042/node/containerlogs/container_1515488762656_0011_01_000001/root");
            var container2 = new ContainerListResult("container_1515488762656_0011_01_000002", new DateTime(2018, 1, 9, 10, 41, 19),
                DateTime.MinValue, RUNNING, _Node1, "http://compute-2:8042/node/containerlogs/container_1515488762656_0011_01_000002/root");
            var container3 = new ContainerListResult("container_1515488762656_0011_01_000003", new DateTime(2018, 1, 9, 10, 41, 19),
                DateTime.MinValue, RUNNING, _Node1, "http://compute-2:8042/node/containerlogs/container_1515488762656_0011_01_000003/root");

            var containers = _Parser.ParseContainerList("");

            Assert.AreEqual(3, containers.Length, "wrong parsed containers count");
            Assert.AreEqual(container1, containers[0], "container1 failed");
            Assert.AreEqual(container2, containers[1], "container2 failed");
            Assert.AreEqual(container3, containers[2], "container3 failed");
        }

        private class DummyHadoopConnector : IHadoopConnector
        {
            public string GetYarnApplicationList(string states) =>
                "application_1515488762656_0001\t  random-text-writer\t           MAPREDUCE\t      root\t   default\t          FINISHED\t         SUCCEEDED\t           100%\thttp://controller:19888/jobhistory/job/job_1515488762656_0001\n" +
                "application_1515488762656_0002\t          word count\t           MAPREDUCE\t      root\t   default\t          FINISHED\t         SUCCEEDED\t           100%\thttp://controller:19888/jobhistory/job/job_1515488762656_0002\n" +
                "application_1515488762656_0003\t              sorter\t           MAPREDUCE\t      root\t   default\t          FINISHED\t         SUCCEEDED\t           100%\thttp://controller:19888/jobhistory/job/job_1515488762656_0003\n";

            public string GetYarnAppAttemptList(string appId) =>
                "appattempt_1515488762656_0002_000001\t            FINISHED\tcontainer_1515488762656_0002_01_000001\thttp://controller:8088/proxy/application_1515488762656_0002/\n";

            public string GetYarnAppContainerList(string attemptId) =>
                "container_1515488762656_0011_01_000001\tTue Jan 09 09:41:14 +0000 2018\t                 N/A\t             RUNNING\t     compute-1:45454\thttp://compute-1:8042\thttp://compute-2:8042/node/containerlogs/container_1515488762656_0011_01_000001/root\n" +
                "container_1515488762656_0011_01_000002\tTue Jan 09 09:41:19 +0000 2018\t                 N/A\t             RUNNING\t     compute-1:45454\thttp://compute-1:8042\thttp://compute-2:8042/node/containerlogs/container_1515488762656_0011_01_000002/root\n" +
                "container_1515488762656_0011_01_000003\tTue Jan 09 09:41:19 +0000 2018\t                 N/A\t             RUNNING\t     compute-1:45454\thttp://compute-1:8042\thttp://compute-2:8042/node/containerlogs/container_1515488762656_0011_01_000003/root\n";

            public string GetYarnApplicationDetails(string appId)
            {
                throw new NotImplementedException();
            }

            public string GetYarnAppAttemptDetails(string attemptId)
            {
                throw new NotImplementedException();
            }

            public string GetYarnAppContainerDetails(string containerId)
            {
                throw new NotImplementedException();
            }

            public string GetYarnNodeList()
            {
                throw new NotImplementedException();
            }

            public string GetYarnNodeDetails(string nodeId)
            {
                throw new NotImplementedException();
            }

            public bool StartNode(string nodeName)
            {
                throw new NotImplementedException();
            }

            public bool StopNode(string nodeName)
            {
                throw new NotImplementedException();
            }

            public bool StopStartNetConnection(string nodeName)
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
}