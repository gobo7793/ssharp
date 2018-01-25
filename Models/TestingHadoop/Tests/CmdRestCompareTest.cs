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
        CmdConnector _Cmd;
        RestConnector _Rest;

        string _Hostname = "swth11";
        string _Username = "siegerge";
        string _PrivKeyFile = @"C:\Users\siegerge\sshnet";

        [TestFixtureSetUp]
        public void Setup()
        {

            var model = new Model();
            model.InitTestConfig(null, null);

            Console.WriteLine("Login to shell");

            var startTime = DateTime.Now;
            _Cmd = new CmdConnector(_Hostname, _Username, _PrivKeyFile, 0, 0);
            var cmdTime = DateTime.Now;
            _Rest = new RestConnector(model, _Hostname, _Username, _PrivKeyFile, 0, 0);
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");
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
        public void TestGetYarnApplicationListTime()
        {
            Console.WriteLine("Get application list");

            if(_Cmd == null || _Rest == null)
                Assert.Ignore();

            var startTime = DateTime.Now;
            _Cmd.GetYarnApplicationList("ALL");
            var cmdTime = DateTime.Now;
            _Rest.GetYarnApplicationList("");
            var restTime = DateTime.Now;

            Console.WriteLine($"CMD needed:  {cmdTime - startTime}");
            Console.WriteLine($"REST needed: {restTime - cmdTime}");

            Assert.Pass();
        }
    }
}