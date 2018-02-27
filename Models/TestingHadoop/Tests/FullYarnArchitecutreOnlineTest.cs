// The MIT License (MIT)
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
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    [TestFixture]
    public class FullYarnArchitecutreOnlineTest
    {
        private Model _Model;
        private YarnApp _App;
        private YarnAppAttempt _Attempt;
        private YarnAppContainer _Container;

        private string _AppBase = "1517215519416_0001";

        [TestFixtureSetUp]
        public void Setup()
        {
            _Model = new Model("Full architecture test");
            _Model.InitModel();

            _App = _Model.Applications[0];
            _App.AppId = $"application_{_AppBase}";

            _Attempt = _App.Attempts[0];
            _Container = _Attempt.Containers[0];
        }

        [Test]
        public void TestFullMonitoring()
        {
            var startTime = DateTime.Now;
            _Model.Controller.MonitorNodes();
            _Model.Clients[0].MonitorApps();
            var elapsedTime = DateTime.Now - startTime;

            Console.WriteLine($"Time needed: {elapsedTime}");

            var app = _Model.Applications.FirstOrDefault(a => a.AppId == $"application_{_AppBase}");

            Assert.AreEqual(ENodeState.RUNNING, _Model.Nodes["compute-1"].State);
            Assert.NotNull(app);

            var attempt = app.Attempts.FirstOrDefault(a => a.AttemptId == $"appattempt_{_AppBase}_000001");
            Assert.NotNull(attempt);

            Assert.AreEqual($"container_{_AppBase}_01_000001", _Attempt.AmContainerId);
            Assert.AreNotEqual(DateTime.MinValue, attempt.AmContainer.StartTime);
        }

        [Test]
        public void TestAppMonitoring()
        {
            var startTime = DateTime.Now;
            _App.MonitorStatus();
            var elapsedTime = DateTime.Now - startTime;

            Console.WriteLine($"Time needed: {elapsedTime}");
            var fullStatus = _App.StatusAsString();
            Console.WriteLine(fullStatus);

            Assert.AreEqual($"appattempt_{_AppBase}_000001", _App.Attempts[0].AttemptId);
            Assert.IsNotNullOrEmpty(_App.Name);
        }

        [Test]
        public void TestAttemptMonitoring()
        {
            if(String.IsNullOrWhiteSpace(_Attempt.AttemptId))
                _Attempt.AttemptId = $"appattempt_{_AppBase}_000001";

            var startTime = DateTime.Now;
            _Attempt.MonitorStatus();
            var elapsedTime = DateTime.Now - startTime;

            Console.WriteLine($"Time needed: {elapsedTime}");
            var fullStatus = _Attempt.StatusAsString();
            Console.WriteLine(fullStatus);

            Assert.AreEqual($"container_{_AppBase}_01_000001", _Attempt.AmContainerId);
            Assert.IsNotNullOrEmpty(_Attempt.AmContainerId);
        }

        [Test]
        public void TestContainerMonitoring()
        {
            if(String.IsNullOrWhiteSpace(_Container.ContainerId))
                _Container.ContainerId = $"container_{_AppBase}_01_000001";

            var startTime = DateTime.Now;
            _Container.MonitorStatus();
            var elapsedTime = DateTime.Now - startTime;

            Console.WriteLine($"Time needed: {elapsedTime}");
            var fullStatus = _Container.StatusAsString();
            Console.WriteLine(fullStatus);

            Assert.AreNotEqual(DateTime.MinValue, _Container.StartTime);
        }
    }
}