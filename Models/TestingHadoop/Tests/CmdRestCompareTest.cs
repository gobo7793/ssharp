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
        private string _AppId = "application_1516703400520_0001";
        private string _AttemptId = "appattempt_1516703400520_0001_000001";
        private string _ContainerId = "container_1516703400520_0001_01_000001";
        private string _NodeId = "compute-1:45454";

        [TestFixtureSetUp]
        public void Setup()
        {

            var model = new Model();
            model.InitTestConfig(null, null);

            Console.WriteLine("Login to shell");

            var startTime = DateTime.Now;
            _Cmd = new CmdConnector(Model.SshHost, Model.SshUsername, Model.SshPrivateKeyFilePath, true, false, 0);
            var cmdTime = DateTime.Now;
            _Rest = new RestConnector(Model.SshHost, Model.SshUsername, Model.SshPrivateKeyFilePath,
                model.Controller.HttpUrl, model.Controller.TimelineHttpUrl);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
            Console.WriteLine();

            Console.WriteLine($"Application id: {_AppId}");
            Console.WriteLine($"Attempt id: {_AttemptId}");
            Console.WriteLine($"Container id: {_ContainerId}");
            Console.WriteLine($"Node id: {_NodeId}");
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

        [Test]
        public void TestGetYarnApplicationList()
        {
            Console.WriteLine("Get application list");

            var startTime = DateTime.Now;
            var cmdOut = _Cmd.GetYarnApplicationList("ALL");
            var cmdTime = DateTime.Now;
            var restOut = _Rest.GetYarnApplicationList("");
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
    }
}