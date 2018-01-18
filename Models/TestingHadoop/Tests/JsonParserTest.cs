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
    public class JsonParserTest
    {
        private JsonParser _Parser;
        private Model _Model;
        private YarnNode _Node1;
        private YarnNode _Node2;
        private YarnNode _Node3;
        private YarnNode _Node4;

        [SetUp]
        public void Setup()
        {
            // Setup modell
            _Model = new Model();
            _Parser = new JsonParser(_Model, new DummyHadoopRestConnector());

            _Model.TestConfig(_Parser, _Parser.Connection);
            _Node1 = _Model.Nodes[$"{Model.NodeNamePrefix}1"];
            _Node2 = _Model.Nodes[$"{Model.NodeNamePrefix}2"];
            _Node3 = _Model.Nodes[$"{Model.NodeNamePrefix}3"];
            _Node4 = _Model.Nodes[$"{Model.NodeNamePrefix}4"];
        }

        [Test]
        public void TestParseAppList()
        {
            #region expected apps
            var app1 = new ApplicationResult
            {
                AppId = "application_1516180206337_0002",
                AppName = "word count",
                State = EAppState.FINISHED,
                FinalStatus = EFinalStatus.SUCCEEDED,
                Progess = 100,
                TrackingUrl = "http://controller:8088/proxy/application_1516180206337_0002/",
                AppType = "MAPREDUCE",
                StartTime = new DateTime(2018, 1, 17, 14, 11, 26, 682),
                FinishTime = new DateTime(2018, 1, 17, 14, 14, 33, 885),
                AmHostHttpAddress = "compute-3:8042",
                AmHost = _Node3,
                AllocatedMb = -1,
                AllocatedVcores = -1,
                RunningContainers = -1,
                MbSeconds = 1726848,
                VcoreSeconds = 1486,
                PreemptedMb = 0,
                PreemptedVcores = 0,
                NonAmContainerPreempted = 0,
                AmContainerPreempted = 0,
            };
            var app2 = new ApplicationResult
            {
                AppId = "application_1516180206337_0001",
                AppName = "random-text-writer",
                State = EAppState.FINISHED,
                FinalStatus = EFinalStatus.SUCCEEDED,
                Progess = 100,
                TrackingUrl = "http://controller:8088/proxy/application_1516180206337_0001/",
                AppType = "MAPREDUCE",
                StartTime = new DateTime(2018, 1, 17, 14, 10, 27, 355),
                FinishTime = new DateTime(2018, 1, 17, 14, 11, 15, 532),
                AmHostHttpAddress = "compute-4:8042",
                AmHost = _Node4,
                AllocatedMb = -1,
                AllocatedVcores = -1,
                RunningContainers = -1,
                MbSeconds = 344054,
                VcoreSeconds = 278,
                PreemptedMb = 0,
                PreemptedVcores = 0,
                NonAmContainerPreempted = 0,
                AmContainerPreempted = 0,
            };
            var app3 = new ApplicationResult
            {
                AppId = "application_1516180206337_0003",
                AppName = "random-text-writer",
                State = EAppState.ACCEPTED,
                FinalStatus = EFinalStatus.UNDEFINED,
                Progess = 0,
                //TrackingUrl = "http://controller:8088/proxy/application_1516180206337_0003/",
                AppType = "MAPREDUCE",
                StartTime = new DateTime(2018, 1, 17, 14, 14, 49, 978),
                FinishTime = DateTime.MinValue,
                AmHostHttpAddress = "compute-1:8042",
                AmHost = _Node1,
                AllocatedMb = 2048,
                AllocatedVcores = 1,
                RunningContainers = 1,
                MbSeconds = 0,
                VcoreSeconds = 0,
                PreemptedMb = 0,
                PreemptedVcores = 0,
                NonAmContainerPreempted = 0,
                AmContainerPreempted = 0,
            };
            #endregion

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
                AttemptId = "1",
                StartTime = new DateTime(2018, 1, 17, 14, 11, 26, 683),
                AmContainerId = "container_1516180206337_0002_01_000001",
                AmHostHttpAddress = "compute-3:8042",
                AmHostId = "compute-3:45454",
                AmHost = _Node3,
                LogsUrl = "//compute-3:8042/node/containerlogs/container_1516180206337_0002_01_000001/root",
            };

            var attempts = _Parser.ParseAppAttemptList("");

            Assert.AreEqual(1, attempts.Length, "wrong parsed attempts count");
            Assert.AreEqual(attempt1, attempts[0], "attempt1 failed");
        }

        [Test]
        public void TestParseNodeList()
        {
            #region expected nodes
            var node1 = new NodeResult
            {
                NodeState = ENodeState.RUNNING,
                NodeId = "compute-1:45454",
                Hostname = "compute-1",
                NodeHttpAdd = "compute-1:8042",
                LastHealthUpdate = new DateTime(2018, 1, 17, 14, 16, 41, 783),
                HealthReport = "",
                RunningContainerCount = 0,
                MemoryUsed = 0,
                MemoryAvailable = 8192,
                CpuUsed = 0,
                CpuAvailable = 8,
            };
            var node2 = new NodeResult
            {
                NodeState = ENodeState.RUNNING,
                NodeId = "compute-2:45454",
                Hostname = "compute-2",
                NodeHttpAdd = "compute-2:8042",
                LastHealthUpdate = new DateTime(2018, 1, 17, 14, 15, 42, 990),
                HealthReport = "",
                RunningContainerCount = 1,
                MemoryUsed = 1024,
                MemoryAvailable = 7168,
                CpuUsed = 1,
                CpuAvailable = 7,
            };
            var node3 = new NodeResult
            {
                NodeState = ENodeState.RUNNING,
                NodeId = "compute-3:45454",
                Hostname = "compute-3",
                NodeHttpAdd = "compute-3:8042",
                LastHealthUpdate = new DateTime(2018, 1, 17, 14, 17, 8, 811),
                HealthReport = "",
                RunningContainerCount = 1,
                MemoryUsed = 1024,
                MemoryAvailable = 7168,
                CpuUsed = 1,
                CpuAvailable = 7,
            };
            var node4 = new NodeResult
            {
                NodeState = ENodeState.RUNNING,
                NodeId = "compute-4:45454",
                Hostname = "compute-4",
                NodeHttpAdd = "compute-4:8042",
                LastHealthUpdate = new DateTime(2018, 1, 17, 14, 16, 00, 656),
                HealthReport = "",
                RunningContainerCount = 7,
                MemoryUsed = 8192,
                MemoryAvailable = 0,
                CpuUsed = 7,
                CpuAvailable = 1,
            };
            var node5 = new NodeResult
            {
                NodeState = ENodeState.LOST,
                NodeId = "compute-5:45454",
                Hostname = "compute-5",
                NodeHttpAdd = "",
                LastHealthUpdate = new DateTime(2018, 1, 17, 10, 27, 14, 249),
                HealthReport = "",
                RunningContainerCount = 0,
                MemoryUsed = 0,
                MemoryAvailable = 0,
                CpuUsed = 0,
                CpuAvailable = 0,
            };
            var node6 = new NodeResult
            {
                NodeState = ENodeState.LOST,
                NodeId = "compute-6:45454",
                Hostname = "compute-6",
                NodeHttpAdd = "",
                LastHealthUpdate = new DateTime(2018, 1, 17, 10, 26, 30, 494),
                HealthReport = "",
                RunningContainerCount = 0,
                MemoryUsed = 0,
                MemoryAvailable = 0,
                CpuUsed = 0,
                CpuAvailable = 0,
            };
            var node7 = new NodeResult
            {
                NodeState = ENodeState.LOST,
                NodeId = "compute-7:45454",
                Hostname = "compute-7",
                NodeHttpAdd = "",
                LastHealthUpdate = new DateTime(2018, 1, 17, 10, 25, 50, 823),
                HealthReport = "",
            };
            var node8 = new NodeResult
            {
                NodeState = ENodeState.LOST,
                NodeId = "compute-8:45454",
                Hostname = "compute-8",
                NodeHttpAdd = "",
                LastHealthUpdate = new DateTime(2018, 1, 17, 10, 25, 18, 458),
                HealthReport = "",
            };
            #endregion

            var nodes = _Parser.ParseNodeList();

            Assert.AreEqual(8, nodes.Length, "wrong parsed node count");
            Assert.AreEqual(node1, nodes[3], "node1 failed");
            Assert.AreEqual(node2, nodes[2], "node2 failed");
            Assert.AreEqual(node3, nodes[1], "node3 failed");
            Assert.AreEqual(node4, nodes[0], "node4 failed");
            Assert.AreEqual(node5, nodes[5], "node5 failed");
            Assert.AreEqual(node6, nodes[4], "node6 failed");
            Assert.AreEqual(node7, nodes[7], "node7 failed");
            Assert.AreEqual(node8, nodes[6], "node8 failed");
        }
    }
}