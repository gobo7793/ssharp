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
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    public class DummyHadoopCmdConnector : IHadoopConnector
    {
        public string GetYarnApplicationList(string states) =>
            "application_1515488762656_0001\t  random-text-writer\t           MAPREDUCE\t      root\t   default\t          FINISHED\t         SUCCEEDED\t           100%\thttp://controller:19888/jobhistory/job/job_1515488762656_0001\n" +
            "application_1515488762656_0002\t          word count\t           MAPREDUCE\t      root\t   default\t          FINISHED\t         SUCCEEDED\t           100%\thttp://controller:19888/jobhistory/job/job_1515488762656_0002\n" +
            "application_1515488762656_0003\t              sorter\t           MAPREDUCE\t      root\t   default\t           RUNNING\t         UNDEFINED\t           67%\thttp://controller:19888/jobhistory/job/job_1515488762656_0003\n";

        public string GetYarnAppAttemptList(string appId) =>
            "appattempt_1515488762656_0002_000001\t            FINISHED\tcontainer_1515488762656_0002_01_000001\thttp://controller:8088/proxy/application_1515488762656_0002/\n";

        public string GetYarnAppAttemptListTl(string appId)
        {
            throw new NotImplementedException();
        }

        public string GetYarnAppContainerList(string attemptId) =>
            "container_1515488762656_0011_01_000001\tTue Jan 09 09:41:14 +0000 2018\t                 N/A\t             RUNNING\t     compute-1:45454\thttp://compute-1:8042\thttp://compute-1:8042/node/containerlogs/container_1515488762656_0011_01_000001/root\n" +
            "container_1515488762656_0011_01_000002\tTue Jan 09 09:41:19 +0000 2018\t                 N/A\t             RUNNING\t     compute-2:45454\thttp://compute-2:8042\thttp://compute-2:8042/node/containerlogs/container_1515488762656_0011_01_000002/root\n" +
            "container_1515488762656_0011_01_000003\tTue Jan 09 09:41:19 +0000 2018\t                 N/A\t             RUNNING\t     compute-1:45454\thttp://compute-1:8042\thttp://compute-1:8042/node/containerlogs/container_1515488762656_0011_01_000003/root\n";

        public string GetYarnAppContainerListTl(string attemptId)
        {
            throw new NotImplementedException();
        }

        public string GetYarnApplicationDetails(string appId) => "Application Report : \n" +
                                                                 "\tApplication-Id : application_1515488762656_0002\n" +
                                                                 "\tApplication-Name : word count\n" +
                                                                 "\tApplication-Type : MAPREDUCE\n" +
                                                                 "\tUser : root\n\tQueue : default\n" +
                                                                 "\tStart-Time : 1515489034402\n" + // new DateTime(2018, 1, 9, 10, 10, 34, 402)
                                                                 "\tFinish-Time : 1515489108249\n" + // new DateTime(2018, 1, 9, 10, 11, 48, 249)
                                                                 "\tProgress : 100%\n" +
                                                                 "\tState : FINISHED\n" +
                                                                 "\tFinal-State : SUCCEEDED\n" +
                                                                 "\tTracking-URL : http://controller:19888/jobhistory/job/job_1515488762656_0002\n" +
                                                                 "\tRPC Port : 38567\n" +
                                                                 "\tAM Host : compute-1\n" +
                                                                 "\tAggregate Resource Allocation : 583396 MB-seconds, 482 vcore-seconds\n" +
                                                                 "\tDiagnostics : \n";

        public string GetYarnAppAttemptDetails(string attemptId) => "Application Attempt Report : \n" +
                                                                    "\tApplicationAttempt-Id : appattempt_1515577485762_0006_000001\n" +
                                                                    "\tState : RUNNING\n" +
                                                                    "\tAMContainer : container_1515577485762_0006_01_000001\n" +
                                                                    "\tTracking-URL : http://controller:8088/proxy/application_1515577485762_0006/\n" +
                                                                    "\tRPC Port : 44340\n" +
                                                                    "\tAM Host : compute-1\n" +
                                                                    "\tDiagnostics : Container released by application\n";

        public string GetYarnAppAttemptDetailsTl(string attemptId)
        {
            throw new NotImplementedException();
        }

        public string GetYarnAppContainerDetails(string containerId) =>
            "[hdp]: Using hadoop console command: yarn container -status container_1516703400520_0002_01_0000014\n" +
            "18/01/23 13:40:22 INFO impl.TimelineClientImpl: Timeline service address: http://0.0.0.0:8188/ws/v1/timeline/\n" +
            "18/01/23 13:40:22 INFO client.RMProxy: Connecting to ResourceManager at controller/10.0.0.3:8032\n" +
            "18/01/23 13:40:22 INFO client.AHSProxy: Connecting to Application History server at /0.0.0.0:10200\n" +
            "Container Report : \n" +
            "\tContainer-Id : container_1515577485762_0008_01_000001\n" +
            "\tStart-Time : 1515579722594\n" + // new DateTime(2018, 1, 10, 11, 22, 2, 594)
            "\tFinish-Time : 0\n" + // DateTime.MinValue
            "\tState : RUNNING\n" +
            "\tLOG-URL : http://compute-1:8042/node/containerlogs/container_1515577485762_0008_01_000001/root\n" +
            "\tHost : compute-1:45454\n" +
            "\tNodeHttpAddress : http://compute-1:8042\n" +
            "\tDiagnostics : Container killed by the ApplicationMaster.\n" +
            "Container killed on request.Exit code is 143\n" +
            "Container exited with a non-zero exit code 143\n";

        public string GetYarnAppContainerDetailsTl(string containerId)
        {
            throw new NotImplementedException();
        }

        public string GetYarnNodeList() => " compute-1:45454\t        RUNNING\t   compute-1:8042\t                           0\n" +
                                           " compute-2:45454\t        RUNNING\t   compute-2:8042\t                           0\n" +
                                           " compute-3:45454\t        RUNNING\t   compute-3:8042\t                           0\n" +
                                           " compute-4:45454\t        RUNNING\t   compute-4:8042\t                           0\n";

        public string GetYarnNodeDetails(string nodeId) => "Node Report : \n" +
                                                           "\tNode-Id : compute-1:45454\n" +
                                                           "\tRack : /default-rack\n" +
                                                           "\tNode-State : RUNNING\n" +
                                                           "\tNode-Http-Address : compute-1:8042\n" +
                                                           "\tLast-Health-Update : Wed 10/Jan/18 10:24:57:291UTC\n" +
                                                           "\tHealth-Report : \n" +
                                                           "\tContainers : 2\n" +
                                                           "\tMemory-Used : 3072MB\n" +
                                                           "\tMemory-Capacity : 8192MB\n" +
                                                           "\tCPU-Used : 2 vcores\n" +
                                                           "\tCPU-Capacity : 8 vcores\n" +
                                                           "\tNode-Labels : \n\n";

        public bool StartNode(string nodeName)
        {
            throw new NotImplementedException();
        }

        public bool StopNode(string nodeName)
        {
            throw new NotImplementedException();
        }

        public bool StartNodeNetConnection(string nodeName)
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