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

using NUnit.Framework;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using static SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel.EAppState;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    [TestFixture]
    public class CmdLineParserTest
    {
        private CmdLineParser _Parser;

        [SetUp]
        public void Setup()
        {
            _Parser = new CmdLineParser(null, new DummyHadoopConnector());
        }

        [Test]
        public void TestParseAppList()
        {
            var app1 = new ApplicationListResult("application_1515149592497_0001", "random-text-writer", "MAPREDUCE",
                FINISHED, "SUCCEEDED", 100, "http://controller:19888/jobhistory/job/job_1515149592497_0001");
            var app2 = new ApplicationListResult("application_1515149592497_0002", "Sleep job", "MAPREDUCE",
                FINISHED, "SUCCEEDED", 100, "http://controller:19888/jobhistory/job/job_1515149592497_0002");
            var app3 = new ApplicationListResult("application_1515149592497_0003", "sorter", "MAPREDUCE",
                FINISHED, "SUCCEEDED", 100, "http://controller:19888/jobhistory/job/job_1515149592497_0003");

            var apps = _Parser.ParseAppList();

            Assert.AreEqual(app1, apps[0], "app1 failed");
            Assert.AreEqual(app2, apps[1], "app2 failed");
            Assert.AreEqual(app3, apps[2], "app3 failed");
        }

        [Test]
        public void TestParseAppAttemptList()
        {
            var attempt1 = new ApplicationAttemptListResult("appattempt_1515143063372_0001_000001", FINISHED,
                "container_1515143063372_0001_01_000001", "http://controller:8088/proxy/application_1515143063372_0001/");

            var attempts = _Parser.ParseAppAttemptList("");

            Assert.AreEqual(attempt1, attempts[0], "attempt1 failed");
        }


        private class DummyHadoopConnector : IHadoopConnector
        {
            public string GetYarnApplicationList(string states)
            {
                return
                    "application_1515149592497_0001    random-text-writer               MAPREDUCE          root         default                FINISHED               SUCCEEDED            100% http://controller:19888/jobhistory/job/job_1515149592497_0001" +
                    "application_1515149592497_0002             Sleep job               MAPREDUCE          root         default                FINISHED               SUCCEEDED            100% http://controller:19888/jobhistory/job/job_1515149592497_0002" +
                    "application_1515149592497_0003                sorter               MAPREDUCE          root         default                FINISHED               SUCCEEDED             10% http://controller:19888/jobhistory/job/job_1515149592497_0003";
            }

            public string GetYarnAppAttemptList(string appId)
            {
                return
                    "appattempt_1515143063372_0001_000001                FINISHED    container_1515143063372_0001_01_000001  http://controller:8088/proxy/application_1515143063372_0001/";
            }

            public string GetYarnAppContainerList(string attemptId)
            {
                return
                    "container_1515149592497_0005_01_000004  Fri Jan 05 11:08:16 +0000 2018                   N/A                 RUNNING         compute-1:45454       http://compute-3:8042   http://compute-3:8042/node/containerlogs/container_1515149592497_0005_01_000004/root" +
                    "container_1515149592497_0005_01_000003  Fri Jan 05 11:08:16 +0000 2018                   N/A                 RUNNING         compute-1:45454       http://compute-4:8042   http://compute-4:8042/node/containerlogs/container_1515149592497_0005_01_000003/root";
            }

            public string GetYarnApplicationDetails(string appId)
            {
                throw new System.NotImplementedException();
            }

            public string GetYarnAppAttemptDetails(string attemptId)
            {
                throw new System.NotImplementedException();
            }

            public string GetYarnAppContainerDetails(string containerId)
            {
                throw new System.NotImplementedException();
            }

            public string GetYarnNodeList()
            {
                throw new System.NotImplementedException();
            }

            public string GetYarnNodeDetails(string nodeId)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}