﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2016, Institute for Software & Systems Engineering
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
using System.Threading;
using NUnit.Framework;
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    [TestFixture]
    public class FullYarnArchitecutreOnlineTest
    {
        private Model _Model;
        private Client _Client1;
        private YarnController _Controller;
        private YarnNode _Node1;
        private YarnNode _Node5;
        private YarnApp _App1;
        private YarnAppAttempt _Attempt1;

        private static ModelSettings.EHostMode _HostMode = ModelSettings.EHostMode.Multihost;
        private static int _HostsCount = 2;
        private static int _NodeBaseCount = 4;
        private string _AppBase1 = "1525869172198_0001";


        [TestFixtureSetUp]
        public void Setup()
        {
            ModelSettings.HostMode = _HostMode;
            ModelSettings.HostsCount = _HostsCount;
            ModelSettings.NodeBaseCount = _NodeBaseCount;
            _Model = Model.Instance;
            _Model.InitModel();
            _Model.Clients[0].BenchController = new BenchmarkController(1);

            _Controller = _Model.Controller;
            _Node1 = _Model.Nodes.First(n => n.Name == $"{ModelSettings.NodeNamePrefix}1");
            _Node5 = _Model.Nodes.FirstOrDefault(n => n.Name == $"{ModelSettings.NodeNamePrefix}5");
            _Client1 = _Model.Clients[0];

            _App1 = _Model.Applications[0];
            _App1.AppId = $"application_{_AppBase1}";

            _Attempt1 = _App1.Attempts[0];
        }

        [Test]
        public void TestBenchSubmitting()
        {
            _Client1.StartBenchmark(BenchmarkController.Benchmarks[1]);
            Console.WriteLine($"Started: {_Client1.CurrentExecutingApp.AppId}");
            //Thread.Sleep(10000);
        }

        [Test]
        public void TestClientSubmitting()
        {
            for(int i = 0; i < 6; i++)
            {
                _Client1.UpdateBenchmark();
                Console.WriteLine($"Bench {i:D2}: {_Client1.BenchController.CurrentBenchmark.Name}, {_Client1.CurrentExecutingApp?.AppId}");
                Thread.Sleep(300);
            }

            Thread.Sleep(7000);
        }

        [Test]
        public void TestFullMonitoring()
        {
            _Attempt1.IsSelfMonitoring = true;
            //var startTime = DateTime.Now;
            _Controller.MonitorNodes();
            _Controller.MonitorApps();
            //var elapsedTime = DateTime.Now - startTime;

            //Console.WriteLine($"Time needed: {elapsedTime}");

            var app = _Model.Applications.FirstOrDefault(a => a.AppId == $"application_{_AppBase1}");

            Assert.AreEqual(ENodeState.RUNNING, _Model.Nodes.First(n => n.Name == $"{ModelSettings.NodeNamePrefix}1").State);
            Assert.NotNull(app);

            var attempt = app.Attempts.FirstOrDefault(a => a.AttemptId == $"appattempt_{_AppBase1}_000001");
            Assert.NotNull(attempt);

            Assert.AreEqual($"container_{_AppBase1}_01_000001", _Attempt1.AmContainerId);
            Assert.AreNotEqual(DateTime.MinValue, attempt.StartTime);
            //Assert.AreNotEqual(DateTime.MinValue, attempt.AmContainer.StartTime);
        }

        [Test]
        public void TestAppMonitoring()
        {
            _App1.IsSelfMonitoring = true;
            //var startTime = DateTime.Now;
            _App1.MonitorStatus();
            //var elapsedTime = DateTime.Now - startTime;

            //Console.WriteLine($"Time needed: {elapsedTime}");
            var fullStatus = _App1.StatusAsString();
            Console.WriteLine(fullStatus);

            Assert.AreEqual($"appattempt_{_AppBase1}_000001", _App1.Attempts[0].AttemptId);
            Assert.IsNotNullOrEmpty(_App1.Name);
        }

        [Test]
        public void TestAttemptMonitoring()
        {
            _Attempt1.IsSelfMonitoring = true;
            if(String.IsNullOrWhiteSpace(_Attempt1.AttemptId))
                _Attempt1.AttemptId = $"appattempt_{_AppBase1}_000001";

            //var startTime = DateTime.Now;
            _Attempt1.MonitorStatus();
            //var elapsedTime = DateTime.Now - startTime;

            //Console.WriteLine($"Time needed: {elapsedTime}");
            var fullStatus = _Attempt1.StatusAsString();
            Console.WriteLine(fullStatus);

            Assert.AreEqual($"container_{_AppBase1}_01_000001", _Attempt1.AmContainerId);
            Assert.IsNotNullOrEmpty(_Attempt1.AmContainerId);
        }

        //[Test]
        //public void TestContainerMonitoring()
        //{
        //    _Container1.IsSelfMonitoring = true;
        //    if(String.IsNullOrWhiteSpace(_Container1.ContainerId))
        //        _Container1.ContainerId = $"container_{_AppBase1}_01_000001";

        //    //var startTime = DateTime.Now;
        //    _Container1.MonitorStatus();
        //    //var elapsedTime = DateTime.Now - startTime;

        //    //Console.WriteLine($"Time needed: {elapsedTime}");
        //    var fullStatus = _Container1.StatusAsString();
        //    Console.WriteLine(fullStatus);

        //    Assert.AreNotEqual(DateTime.MinValue, _Container1.StartTime);
        //}

        [Test]
        public void TestStartStopCluster()
        {
            var isStarted = Model.Instance.UsingFaultingConnector.StartCluster();
            Assert.IsTrue(isStarted);
            Thread.Sleep(15000);
            var isStopped = Model.Instance.UsingFaultingConnector.StopCluster();
            Assert.IsTrue(isStopped);
        }

        [Test]
        public void TestStopNodeOnHost1()
        {
            Console.WriteLine("Stop node on host 1...");
            var isStopped = _Node1.StopNode();
            Assert.IsTrue(isStopped);
            Thread.Sleep(15000);
            Console.WriteLine("Start node on host 1...");
            var isStarted = _Node1.StartNode();
            Assert.IsTrue(isStarted);
        }

        [Test]
        public void TestStopNodeOnHost2()
        {
            if(_Node5 == null)
                return;

            Console.WriteLine("Stop node on host 2...");
            var isStopped = _Node5.StopNode();
            Assert.IsTrue(isStopped);
            Thread.Sleep(15000);
            Console.WriteLine("Start node on host 2...");
            var isStarted = _Node5.StartNode();
            Assert.IsTrue(isStarted);
        }

        [Test]
        public void TestStopNodeConnectionOnNode1()
        {
            Console.WriteLine("Stop node connection on host 1...");
            var isStopped = _Node1.StopConnection();
            Assert.IsTrue(isStopped);
            Thread.Sleep(15000);
            Console.WriteLine("Start node connection on host 1...");
            var isStarted = _Node1.StartConnection();
            Assert.IsTrue(isStarted);
        }

        [Test]
        public void TestStopNodeConnectionOnNode2()
        {
            if(_Node5 == null)
                return;

            Console.WriteLine("Stop node connection on host 2...");
            var isStopped = _Node5.StopConnection();
            Assert.IsTrue(isStopped);
            Thread.Sleep(15000);
            Console.WriteLine("Start node connection on host 2...");
            var isStarted = _Node5.StartConnection();
            Assert.IsTrue(isStarted);
        }

        [Test]
        public void TestGetMarpValue()
        {
            Console.WriteLine("Getting MARP value...");
            _Controller.MonitorMarp();
            var first=_Controller.MarpValues.FirstOrDefault();
            Console.WriteLine($"First MARP: {first}");
            Assert.AreEqual(0.1, first);
        }
    }
}