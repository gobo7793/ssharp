﻿// The MIT License (MIT)
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
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    public class DummyHadoopConnector : IHadoopConnector
    {
        public string GetYarnApplicationList(string states) =>
            "application_1515488762656_0001\t  random-text-writer\t           MAPREDUCE\t      root\t   default\t          FINISHED\t         SUCCEEDED\t           100%\thttp://controller:19888/jobhistory/job/job_1515488762656_0001\n" +
            "application_1515488762656_0002\t          word count\t           MAPREDUCE\t      root\t   default\t          FINISHED\t         SUCCEEDED\t           100%\thttp://controller:19888/jobhistory/job/job_1515488762656_0002\n" +
            "application_1515488762656_0003\t              sorter\t           MAPREDUCE\t      root\t   default\t          FINISHED\t         SUCCEEDED\t           100%\thttp://controller:19888/jobhistory/job/job_1515488762656_0003\n";

        public string GetYarnAppAttemptList(string appId) =>
            "appattempt_1515488762656_0002_000001\t            FINISHED\tcontainer_1515488762656_0002_01_000001\thttp://controller:8088/proxy/application_1515488762656_0002/\n";

        public string GetYarnAppContainerList(string attemptId) =>
            "container_1515488762656_0011_01_000001\tTue Jan 09 09:41:14 +0000 2018\t                 N/A\t             RUNNING\t     compute-1:45454\thttp://compute-1:8042\thttp://compute-2:8042/node/containerlogs/container_1515488762656_0011_01_000001/root\n" +
            "container_1515488762656_0011_01_000002\tTue Jan 09 09:41:19 +0000 2018\t                 N/A\t             RUNNING\t     compute-1:45454\thttp://compute-1:8042\thttp://compute-2:8042/node/containerlogs/container_1515488762656_0011_01_000002/root\n" +
            "container_1515488762656_0011_01_000003\tTue Jan 09 09:41:19 +0000 2018\t                 N/A\t             RUNNING\t     compute-1:45454\thttp://compute-1:8042\thttp://compute-2:8042/node/containerlogs/container_1515488762656_0011_01_000003/root\n";

        public string GetYarnApplicationDetails(string appId)
        {
            throw new NotImplementedException();
        }

        public string GetYarnAppAttemptDetails(string attemptId)
        {
            throw new NotImplementedException();
        }

        public string GetYarnAppContainerDetails(string containerId)
        {
            throw new NotImplementedException();
        }

        public string GetYarnNodeList()
        {
            throw new NotImplementedException();
        }

        public string GetYarnNodeDetails(string nodeId)
        {
            throw new NotImplementedException();
        }

        public bool StartNode(string nodeName)
        {
            throw new NotImplementedException();
        }

        public bool StopNode(string nodeName)
        {
            throw new NotImplementedException();
        }

        public bool StopStartNetConnection(string nodeName)
        {
            throw new NotImplementedException();
        }

        public bool StopNodeNetConnection(string nodeName)
        {
            throw new NotImplementedException();
        }

        public bool KillApplication(string appId)
        {
            throw new NotImplementedException();
        }
    }
}