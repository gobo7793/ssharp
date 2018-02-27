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
using System.Threading;
using NUnit.Framework;
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Connector;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    /// <summary>
    /// Tests for comparing speed of cmd line access and REST API for Monitoring Hadoop
    /// </summary>
    [TestFixture]
    public class CmdRestCompareTest
    {
        private CmdConnector _Cmd;
        private RestConnector _Rest;

        // After restart cluster and executing applications change these values for the app!
        private string _AppId = "application_1517215519416_0001";
        private string _AttemptId = "appattempt_1517215519416_0001_000001";
        private string _ContainerId = "container_1517215519416_0001_01_000001";
        private string _NodeId = "compute-1:45454";
        // For Fault Handling
        private string _FaultNodeName = "compute-4";
        private string _FaultAppId = "application_1517215519416_0011";

        [TestFixtureSetUp]
        public void Setup()
        {

            var model = new Model();
            model.InitTestConfig(null, null);

            Console.WriteLine("Login to shell");

            var startTime = DateTime.Now;
            _Cmd = new CmdConnector(Model.SshHost, Model.SshUsername, Model.SshPrivateKeyFile, true, true, 1);
            var cmdTime = DateTime.Now;
            _Rest = new RestConnector(Model.SshHost, Model.SshUsername, Model.SshPrivateKeyFile,
                model.Controller.HttpUrl, model.Controller.TimelineHttpUrl);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"Application id: {_AppId}");
            Console.WriteLine($"Attempt id: {_AttemptId}");
            Console.WriteLine($"Container id: {_ContainerId}");
            Console.WriteLine($"Node id: {_NodeId}");
            Console.WriteLine();
            Console.WriteLine($"Fault Node name: {_FaultNodeName}");
            Console.WriteLine($"Fault Application id: {_FaultAppId}");
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            _Cmd = null;
            _Rest = null;
        }

        [SetUp]
        public void OnTestStarting()
        {
            Console.Write("Comparing CMD Line and REST API:");
        }

        #region Monitoring

        [Test]
        public void TestGetYarnApplicationList()
        {
            Console.WriteLine("Get application list");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnApplicationList("ALL");
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnApplicationList("None");
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine($"REST Output:\n{restOut}");

            Assert.Pass();
        }

        [Test]
        public void TestGetYarnAppAttemptList()
        {
            Console.WriteLine("Get attempt list");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnAppAttemptList(_AppId);
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnAppAttemptList(_AppId);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine($"REST Output:\n{restOut}");

            Assert.Pass();
        }

        [Test]
        public void TestGetYarnAppAttemptListTl()
        {
            Console.WriteLine("Get timeline attempt list");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnAppAttemptListTl(_AppId);
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnAppAttemptListTl(_AppId);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine($"REST Output:\n{restOut}");

            Assert.Pass();
        }

        [Test]
        public void TestGetYarnAppContainerList()
        {
            Console.WriteLine("Get container list");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnAppContainerList(_AttemptId);
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnAppContainerList(_AttemptId);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine($"REST Output:\n{restOut}");

            Assert.Pass();
        }

        [Test]
        public void TestGetYarnAppContainerListTl()
        {
            Console.WriteLine("Gettimeline  container list");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnAppContainerListTl(_AttemptId);
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnAppContainerListTl(_AttemptId);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine($"REST Output:\n{restOut}");

            Assert.Pass();
        }

        [Test]
        public void TestGetYarnApplicationDetails()
        {
            Console.WriteLine("Get application details");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnApplicationDetails(_AppId);
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnApplicationDetails(_AppId);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine($"REST Output:\n{restOut}");

            Assert.Pass();
        }

        [Test]
        public void TestGetYarnAppAttemptDetails()
        {
            Console.WriteLine("Get attempt details");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnAppAttemptDetails(_AttemptId);
            var cmdTime = DateTime.Now;
            //var restOut = _Rest.GetYarnAppAttemptDetails(_AttemptId);
            //var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            //Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine("REST Output: N/A");

            Assert.Pass();
        }

        [Test]
        public void TestGetYarnAppAttemptDetailsTl()
        {
            Console.WriteLine("Get timeline attempt details");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnAppAttemptDetailsTl(_AttemptId);
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnAppAttemptDetailsTl(_AttemptId);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine($"REST Output:\n{restOut}");

            Assert.Pass();
        }

        [Test]
        public void TestGetYarnAppContainerDetails()
        {
            Console.WriteLine("Get container details");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnAppContainerDetails(_ContainerId);
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnAppContainerDetails(_ContainerId, _NodeId);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine($"REST Output:\n{restOut}");

            Assert.Pass();
        }

        [Test]
        public void TestGetYarnAppContainerDetailsTl()
        {
            Console.WriteLine("Get timeline container details");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnAppContainerDetailsTl(_ContainerId);
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnAppContainerDetailsTl(_ContainerId);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine($"REST Output:\n{restOut}");

            Assert.Pass();
        }

        [Test]
        public void TestGetYarnNodeList()
        {
            Console.WriteLine("Get node list");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnNodeList();
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnNodeList();
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine($"REST Output:\n{restOut}");

            Assert.Pass();
        }

        [Test]
        public void TestGetYarnNodeDetails()
        {
            Console.WriteLine("Get node details");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnNodeDetails(_NodeId);
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnNodeDetails(_NodeId);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"CMD Output:\n{cmdOut}");
            Console.WriteLine();
            Console.WriteLine($"REST Output:\n{restOut}");

            Assert.Pass();
        }

        #endregion

        #region Faulting/Submitting

        [Test]
        public void TestStopAndStartNode()
        {
            Console.WriteLine("Stop and start node");

            var startTime = DateTime.Now;
            var stopOut = _Cmd.StopNode(_FaultNodeName);
            var stoppingTime = DateTime.Now - startTime;
            Thread.Sleep(500);
            startTime = DateTime.Now;
            var startOut = _Cmd.StartNode(_FaultNodeName);
            var startingTime = DateTime.Now - startTime;

            Console.WriteLine($"Stop needed:  {stoppingTime}");
            Console.WriteLine($"Start needed: {startingTime}");
            Console.WriteLine();

            Console.WriteLine($"Stop Output:\n{stopOut}");
            Console.WriteLine();
            Console.WriteLine($"Start Output:\n{startOut}");

            Assert.IsTrue(stopOut);
            Assert.IsTrue(startOut);
        }

        [Test]
        public void TestStopAndStartNodeNetwork()
        {
            Console.WriteLine("Stop and start node network connection");

            var startTime = DateTime.Now;
            var stopOut = _Cmd.StopNodeNetConnection(_FaultNodeName);
            var stoppingTime = DateTime.Now - startTime;
            Thread.Sleep(500);
            startTime = DateTime.Now;
            var startOut = _Cmd.StartNodeNetConnection(_FaultNodeName);
            var startingTime = DateTime.Now - startTime;

            Console.WriteLine($"Stop needed:  {stoppingTime}");
            Console.WriteLine($"Start needed: {startingTime}");
            Console.WriteLine();

            Console.WriteLine($"Stop Output:\n{stopOut}");
            Console.WriteLine();
            Console.WriteLine($"Start Output:\n{startOut}");

            Assert.IsTrue(stopOut);
            Assert.IsTrue(startOut);
        }

        [Test]
        public void TestStartAndKillingApplication()
        {
            Console.WriteLine("Start and killing application");

            var startTime = DateTime.Now;
            _Cmd.StartApplication("pi");
            var stoppingTime = DateTime.Now - startTime;
            Console.WriteLine("waiting...");
            Thread.Sleep(35000);
            startTime = DateTime.Now;
            var stopOut = _Cmd.KillApplication(_FaultAppId);
            var startingTime = DateTime.Now - startTime;

            Console.WriteLine($"Start needed:  {stoppingTime}");
            Console.WriteLine($"Kill needed: {startingTime}");
            Console.WriteLine();

            Console.WriteLine("Start Output: N/A");
            Console.WriteLine();
            Console.WriteLine($"Kill Output:\n{stopOut}");

            Assert.IsTrue(stopOut);
        }

        #endregion
    }
}