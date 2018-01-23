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
                AppId = "application_1516703400520_0003",
                AppName = "word count",
                State = EAppState.FINISHED,
                FinalStatus = EFinalStatus.SUCCEEDED,
                Progess = 100,
                TrackingUrl = "http://controller:8088/proxy/application_1516703400520_0003/",
                AppType = "MAPREDUCE",
                StartTime = new DateTime(2018, 1, 23, 11, 50, 45, 743),
                FinishTime = new DateTime(2018, 1, 23, 11, 51, 32, 320),
                AmHostHttpAddress = "compute-1:8042",
                AmHost = _Node1,
                AllocatedMb = -1,
                AllocatedVcores = -1,
                RunningContainers = -1,
                MbSeconds = 406772,
                VcoreSeconds = 338,
                PreemptedMb = 0,
                PreemptedVcores = 0,
                NonAmContainerPreempted = 0,
                AmContainerPreempted = 0,
            };
            var app2 = new ApplicationResult
            {
                AppId = "application_1516703400520_0004",
                AppName = "random-text-writer",
                State = EAppState.FINISHED,
                FinalStatus = EFinalStatus.SUCCEEDED,
                Progess = 100,
                TrackingUrl = "http://controller:8088/proxy/application_1516703400520_0004/",
                AppType = "MAPREDUCE",
                StartTime = new DateTime(2018, 1, 23, 11, 51, 47, 421),
                FinishTime = new DateTime(2018, 1, 23, 11, 52, 20, 444),
                AmHostHttpAddress = "compute-2:8042",
                AmHost = _Node2,
                AllocatedMb = -1,
                AllocatedVcores = -1,
                RunningContainers = -1,
                MbSeconds = 196821,
                VcoreSeconds = 148,
                PreemptedMb = 0,
                PreemptedVcores = 0,
                NonAmContainerPreempted = 0,
                AmContainerPreempted = 0,
            };
            var app3 = new ApplicationResult
            {
                AppId = "application_1516703400520_0002",
                AppName = "random-text-writer",
                State = EAppState.FINISHED,
                FinalStatus = EFinalStatus.SUCCEEDED,
                Progess = 100,
                TrackingUrl = "http://controller:8088/proxy/application_1516703400520_0002/",
                AppType = "MAPREDUCE",
                StartTime = new DateTime(2018, 1, 23, 11, 49, 38, 683),
                FinishTime = new DateTime(2018, 1, 23, 11, 50, 35, 000),
                AmHostHttpAddress = "compute-1:8042",
                AmHost = _Node1,
                AllocatedMb = -1,
                AllocatedVcores = -1,
                RunningContainers = -1,
                MbSeconds = 351588,
                VcoreSeconds = 272,
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
            #region expected attempts
            var attempt1 = new ApplicationAttemptResult
            {
                AttemptId = "appattempt_1516703400520_0010_000001",
                StartTime = new DateTime(2018, 1, 23, 15, 22, 59, 491),
                AmContainerId = "container_1516703400520_0010_01_000001",
                AmHostHttpAddress = "compute-1:8042",
                AmHostId = "compute-1:45454",
                AmHost = _Node1,
                LogsUrl = "//compute-1:8042/node/containerlogs/container_1516703400520_0010_01_000001/root",
                TrackingUrl = "http://controller:8088/cluster/app/application_1516703400520_0010",
                Diagnostics = "ApplicationMaster for attempt appattempt_1516703400520_0010_000001 timed out",
                State = EAppState.FAILED,
            };
            var attempt2 = new ApplicationAttemptResult
            {
                AttemptId = "appattempt_1516703400520_0010_000002",
                StartTime = new DateTime(2018, 1, 23, 15, 23, 10, 696),
                AmContainerId = "container_1516703400520_0010_02_000001",
                AmHostHttpAddress = "compute-1:8042",
                AmHostId = "compute-1:45454",
                AmHost = _Node1,
                LogsUrl = "//compute-1:8042/node/containerlogs/container_1516703400520_0010_02_000001/root",
                TrackingUrl = "http://controller:8088/proxy/application_1516703400520_0010/",
                Diagnostics = "",
                State = EAppState.FINISHED,
            };
            #endregion

            var attempts = _Parser.ParseAppAttemptList("application_1516703400520_0010");

            Assert.AreEqual(2, attempts.Length, "wrong parsed attempts count");
            Assert.AreEqual(attempt1, attempts[0], "attempt1 failed");
            Assert.AreEqual(attempt2, attempts[1], "attempt2 failed");
        }

        [Test]
        public void TestParseContainerList()
        {

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
                LastHealthUpdate = new DateTime(2018, 1, 23, 15, 12, 54, 105),
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
                LastHealthUpdate = new DateTime(2018, 1, 23, 15, 12, 59, 252),
                HealthReport = "",
                RunningContainerCount = 0,
                MemoryUsed = 0,
                MemoryAvailable = 8192,
                CpuUsed = 0,
                CpuAvailable = 8,
            };
            var node3 = new NodeResult
            {
                NodeState = ENodeState.RUNNING,
                NodeId = "compute-3:45454",
                Hostname = "compute-3",
                NodeHttpAdd = "compute-3:8042",
                LastHealthUpdate = new DateTime(2018, 1, 23, 15, 13, 5, 514),
                HealthReport = "",
                RunningContainerCount = 0,
                MemoryUsed = 0,
                MemoryAvailable = 8192,
                CpuUsed = 0,
                CpuAvailable = 8,
            };
            var node4 = new NodeResult
            {
                NodeState = ENodeState.RUNNING,
                NodeId = "compute-4:45454",
                Hostname = "compute-4",
                NodeHttpAdd = "compute-4:8042",
                LastHealthUpdate = new DateTime(2018, 1, 23, 15, 13, 22, 910),
                HealthReport = "",
                RunningContainerCount = 0,
                MemoryUsed = 0,
                MemoryAvailable = 8192,
                CpuUsed = 0,
                CpuAvailable = 8,
            };
            #endregion

            var nodes = _Parser.ParseNodeList();

            Assert.AreEqual(4, nodes.Length, "wrong parsed node count");
            Assert.AreEqual(node1, nodes[3], "node1 failed");
            Assert.AreEqual(node2, nodes[2], "node2 failed");
            Assert.AreEqual(node3, nodes[1], "node3 failed");
            Assert.AreEqual(node4, nodes[0], "node4 failed");
        }

        [Test]
        public void TestParseApplicationDetails()
        {
            var app = new ApplicationResult
            {
                AppId = "application_1516180206337_0004",
                AppName = "sorter",
                State = EAppState.RUNNING,
                FinalStatus = EFinalStatus.UNDEFINED,
                Progess = 5,
                TrackingUrl = "http://controller:8088/proxy/application_1516180206337_0004/",
                AppType = "MAPREDUCE",
                StartTime = new DateTime(2018, 1, 17, 14, 15, 30, 978),
                FinishTime = DateTime.MinValue,
                AmHostHttpAddress = "compute-4:8042",
                AmHost = _Node4,
                AllocatedMb = 10240,
                AllocatedVcores = 9,
                RunningContainers = 9,
                MbSeconds = 279543,
                VcoreSeconds = 234,
                PreemptedMb = 0,
                PreemptedVcores = 0,
                NonAmContainerPreempted = 0,
                AmContainerPreempted = 0,
            };

            var res = _Parser.ParseAppDetails("");

            Assert.AreEqual(app, res);
        }

        [Test]
        public void TestParseAppAttemptDetails()
        {

        }

        [Test]
        public void TestParseContainerDetails()
        {

        }

        [Test]
        public void TestParseNodeDetails()
        {
            var running = new NodeResult
            {
                NodeState = ENodeState.RUNNING,
                NodeId = "compute-3:45454",
                Hostname = "compute-3",
                NodeHttpAdd = "compute-3:8042",
                LastHealthUpdate = new DateTime(2018, 1, 17, 14, 14, 48, 21),
                HealthReport = "",
                RunningContainerCount = 2,
                MemoryUsed = 2048,
                MemoryAvailable = 6144,
                CpuUsed = 2,
                CpuAvailable = 6,
            };
            var dead = new NodeResult
            {
                NodeState = ENodeState.LOST,
                NodeId = "compute-6:45454",
                Hostname = "compute-6",
                NodeHttpAdd = "",
                LastHealthUpdate = new DateTime(2018, 1, 17, 10, 26, 30, 494),
                HealthReport = "",
            };

            var runningRes = _Parser.ParseNodeDetails("");
            var deadRes = _Parser.ParseNodeDetails("dead");

            Assert.AreEqual(running, runningRes, "running node failed");
            Assert.AreEqual(dead, deadRes, "dead node failed");
        }
    }
}