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
using NUnit.Framework;
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Parser;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    [TestFixture]
    public class FullYarnArchitectureOfflineTest
    {
        private Model _Model;
        private Client _Client1;
        private YarnApp _App;
        private YarnAppAttempt _Attempt;
        private YarnAppContainer _Container;

        [TestFixtureSetUp]
        public void Setup()
        {
            _Model = new Model();
            var parser = new CmdParser(_Model, new DummyHadoopCmdConnector());

            _Model.InitTestConfig(parser, parser.Connection, 1);

            _Client1 = _Model.Clients[0];
            _App = _Model.Applications[0];
            _App.AppId = "application_1515488762656_0002";

            _Attempt = _App.Attempts[0];
            _Container = _Attempt.Containers[0];
        }

        [Test]
        public void TestBenchSubmitting()
        {
            for(int i = 0; i < 5; i++)
            {
                _Client1.UpdateBenchmark();
                Console.WriteLine($"Bench {i:D2}: {_Client1.BenchController.CurrentBenchmark.Name}");
            }
        }

        [Test]
        public void TestGetAppStatus()
        {
            _App.MonitorStatus();

            Assert.AreEqual("application_1515488762656_0002", _App.AppId);
            Assert.AreEqual(new DateTime(2018, 1, 9, 10, 10, 34, 402), _App.StartTime);
            Assert.AreEqual(_Model.Nodes["compute-1"], _App.AmHost);
            Assert.AreEqual(1, _App.Attempts.Count(a => !String.IsNullOrWhiteSpace(a.AttemptId)));
            Assert.AreEqual("appattempt_1515488762656_0002_000001", _App.Attempts[0].AttemptId);
        }

        [Test]
        public void TestGetAttemptStatus()
        {
            _Attempt.AttemptId = "appattempt_1515577485762_0006_000001";

            _Attempt.MonitorStatus();

            Assert.AreEqual("appattempt_1515577485762_0006_000001", _Attempt.AttemptId);
            Assert.AreEqual("container_1515577485762_0006_01_000001", _Attempt.AmContainerId);
            Assert.AreEqual(_Model.Nodes[$"{Model.NodeNamePrefix}1"], _Attempt.AmHost);
            Assert.AreEqual(3, _Attempt.Containers.Count(c => !String.IsNullOrWhiteSpace(c.ContainerId)));
            Assert.AreEqual("container_1515488762656_0011_01_000002", _Attempt.Containers[1].ContainerId);
        }

        [Test]
        public void TestGetContainerStatus()
        {
            _Container.ContainerId = "container_1515577485762_0008_01_000001";

            _Container.MonitorStatus();

            Assert.AreEqual("container_1515577485762_0008_01_000001", _Container.ContainerId);
            Assert.AreEqual(new DateTime(2018, 1, 10, 11, 22, 2, 594), _Container.StartTime);
            Assert.AreEqual(_Model.Nodes[$"{Model.NodeNamePrefix}1"], _Container.Host);
        }
    }
}