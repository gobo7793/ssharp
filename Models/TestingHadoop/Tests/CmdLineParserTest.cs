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
using NUnit.Framework;
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    [TestFixture]
    public class CmdLineParserTest
    {
        private CmdLineParser _Parser;
        private Model _Model;
        private YarnNode _Node1;
        private YarnNode _Node2;

        [SetUp]
        public void Setup()
        {
            _Model = new Model();
            _Parser = new CmdLineParser(_Model, new DummyHadoopCmdConnector());

            _Model.TestConfig(_Parser, _Parser.Connection);
            _Node1 = _Model.Nodes[$"{Model.NodeNamePrefix}1"];
            _Node2 = _Model.Nodes[$"{Model.NodeNamePrefix}2"];

        }

        [Test]
        public void TestParseAppList()
        {
            var app1 = new ApplicationResult
            {
                AppId = "application_1515488762656_0001",
                AppName = "random-text-writer",
                AppType = "MAPREDUCE",
                State = EAppState.FINISHED,
                FinalStatus = EFinalStatus.SUCCEEDED,
                Progess = 100,
                TrackingUrl = "http://controller:19888/jobhistory/job/job_1515488762656_0001",
            };
            var app2 = new ApplicationResult
            {
                AppId = "application_1515488762656_0002",
                AppName = "word count",
                AppType = "MAPREDUCE",
                State = EAppState.FINISHED,
                FinalStatus = EFinalStatus.SUCCEEDED,
                Progess = 100,
                TrackingUrl = "http://controller:19888/jobhistory/job/job_1515488762656_0002",
            };
            var app3 = new ApplicationResult
            {
                AppId = "application_1515488762656_0003",
                AppName = "sorter",
                AppType = "MAPREDUCE",
                State = EAppState.RUNNING,
                FinalStatus = EFinalStatus.UNDEFINED,
                Progess = 67,
                TrackingUrl = "http://controller:19888/jobhistory/job/job_1515488762656_0003",
            };

            var apps = _Parser.ParseAppList();

            Assert.AreEqual(3, apps.Length, "wrong parsed apps count");
            Assert.AreEqual(app1, apps[0], "app1 failed");
            Assert.AreEqual(app2, apps[1], "app2 failed");
            Assert.AreEqual(app3, apps[2], "app3 failed");
        }

        [Test]
        public void TestParseAppAttemptList()
        {
            var attempt1 = new ApplicationAttemptResult
            {
                AttemptId = "appattempt_1515488762656_0002_000001",
                State = EAppState.FINISHED,
                AmContainerId = "container_1515488762656_0002_01_000001",
                TrackingUrl = "http://controller:8088/proxy/application_1515488762656_0002/"
            };

            var attempts = _Parser.ParseAppAttemptList("");

            Assert.AreEqual(1, attempts.Length, "wrong parsed attempts count");
            Assert.AreEqual(attempt1, attempts[0], "attempt1 failed");
        }

        [Test]
        public void TestParseContainerList()
        {
            var container1 = new ContainerResult
            {
                ContainerId = "container_1515488762656_0011_01_000001",
                StartTime = new DateTime(2018, 1, 9, 10, 41, 14),
                FinishTime = DateTime.MinValue,
                State = EContainerState.RUNNING,
                Host = _Node1,
                LogUrl = "http://compute-1:8042/node/containerlogs/container_1515488762656_0011_01_000001/root",
            };
            var container2 = new ContainerResult
            {
                ContainerId = "container_1515488762656_0011_01_000002",
                StartTime = new DateTime(2018, 1, 9, 10, 41, 19),
                FinishTime = DateTime.MinValue,
                State = EContainerState.RUNNING,
                Host = _Node2,
                LogUrl = "http://compute-2:8042/node/containerlogs/container_1515488762656_0011_01_000002/root",
            };
            var container3 = new ContainerResult
            {
                ContainerId = "container_1515488762656_0011_01_000003",
                StartTime = new DateTime(2018, 1, 9, 10, 41, 19),
                FinishTime = DateTime.MinValue,
                State = EContainerState.RUNNING,
                Host = _Node1,
                LogUrl = "http://compute-1:8042/node/containerlogs/container_1515488762656_0011_01_000003/root",
            };

            var containers = _Parser.ParseContainerList("");

            Assert.AreEqual(3, containers.Length, "wrong parsed containers count");
            Assert.AreEqual(container1, containers[0], "container1 failed");
            Assert.AreEqual(container2, containers[1], "container2 failed");
            Assert.AreEqual(container3, containers[2], "container3 failed");
        }

        [Test]
        public void TestParseNodeList()
        {
            var node1 = new NodeResult
            {
                NodeId = "compute-1:45454",
                NodeState = ENodeState.RUNNING,
                NodeHttpAdd = "compute-1:8042",
                RunningContainerCount = 0,
            };
            var node2 = new NodeResult
            {
                NodeId = "compute-2:45454",
                NodeState = ENodeState.RUNNING,
                NodeHttpAdd = "compute-2:8042",
                RunningContainerCount = 0,
            };
            var node3 = new NodeResult
            {
                NodeId = "compute-3:45454",
                NodeState = ENodeState.RUNNING,
                NodeHttpAdd = "compute-3:8042",
                RunningContainerCount = 0,
            };
            var node4 = new NodeResult
            {
                NodeId = "compute-4:45454",
                NodeState = ENodeState.RUNNING,
                NodeHttpAdd = "compute-4:8042",
                RunningContainerCount = 0,
            };

            var nodes = _Parser.ParseNodeList();

            Assert.AreEqual(4, nodes.Length, "wrong parsed node count");
            Assert.AreEqual(node1, nodes[0], "node1 failed");
            Assert.AreEqual(node2, nodes[1], "node2 failed");
            Assert.AreEqual(node3, nodes[2], "node3 failed");
            Assert.AreEqual(node4, nodes[3], "node4 failed");
        }

        [Test]
        public void TestParseAppDetails()
        {
            var app = new ApplicationResult
            {
                AppId = "application_1515488762656_0002",
                AppName = "word count",
                AppType = "MAPREDUCE",
                State = EAppState.FINISHED,
                FinalStatus = EFinalStatus.SUCCEEDED,
                Progess = 100,
                TrackingUrl = "http://controller:19888/jobhistory/job/job_1515488762656_0002",
                StartTime = new DateTime(2018, 1, 9, 10, 10, 34, 402),
                FinishTime = new DateTime(2018, 1, 9, 10, 11, 48, 249),
                AmHost = _Node1,
                MbSeconds = 583396,
                VcoreSeconds = 482,
            };

            var res = _Parser.ParseAppDetails("");

            Assert.AreEqual(app, res);
        }

        [Test]
        public void TestParseAppAttemptDetails()
        {
            var attempt = new ApplicationAttemptResult
            {
                AttemptId = "appattempt_1515577485762_0006_000001",
                State = EAppState.RUNNING,
                AmContainerId = "container_1515577485762_0006_01_000001",
                TrackingUrl = "http://controller:8088/proxy/application_1515577485762_0006/",
                AmHost = _Node1,
            };

            var res = _Parser.ParseAppAttemptDetails("");

            Assert.AreEqual(attempt, res);
        }

        [Test]
        public void TestParseContainerDetails()
        {
            var container = new ContainerResult
            {
                ContainerId = "container_1515577485762_0008_01_000001",
                StartTime = new DateTime(2018, 1, 10, 11, 22, 2, 594),
                FinishTime = DateTime.MinValue,
                State = EContainerState.RUNNING,
                Host = _Node1,
                LogUrl = "http://compute-1:8042/node/containerlogs/container_1515577485762_0008_01_000001/root",
            };

            var res = _Parser.ParseContainerDetails("");

            Assert.AreEqual(container, res);
        }

        [Test]
        public void TestParseNodeDetails()
        {
            var node = new NodeResult
            {
                NodeId = "compute-1:45454",
                NodeState = ENodeState.RUNNING,
                NodeHttpAdd = "compute-1:8042",
                RunningContainerCount = 2,
                CpuUsed = 2,
                CpuCapacity = 8,
                MemoryUsed = 3072,
                MemoryCapacity = 8192,
            };

            var res = _Parser.ParseNodeDetails("");

            Assert.AreEqual(node, res);
        }
    }
}