#region License
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
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Connector;

namespace SafetySharp.CaseStudies.TestingHadoop.Analysis
{
    [TestFixture]
    public class CaseStudyTests
    {

        #region Settings

        private static log4net.ILog Logger { get; } =
            log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Indicates if connectors are initialized
        /// </summary>
        public bool IsInitConnectors { get; set; }

        /// <summary>
        /// Mutation scenario name
        /// </summary>
        public string MutationConfig { get; set; } = "mut";

        #endregion

        #region Preparing

        /// <summary>
        /// Generate the case study seeds
        /// </summary>
        [Test]
        public void GenerateCaseStudyBenchSeeds()
        {
            var ticks = Environment.TickCount;
            var ran = new Random(ticks);
            var s1 = ran.Next(0, int.MaxValue);
            var s2 = ran.Next(0, int.MaxValue);
            Console.WriteLine($"Ticks: 0x{ticks:X}");
            Console.WriteLine($"s1: 0x{s1:X} | s2: 0x{s2:X}");
            // Specific output for generating test case seeds:
            // Ticks: 0xC426B8
            // s1: 0x36159C73 | s2: 0x60E70223
        }

        #endregion

        #region Test cases

        /// <summary>
        /// Returns the test cases for nunit
        /// </summary>
        /// <returns>A test case</returns>
        public IEnumerable GetTestCases()
        {
            return from seed in GetSeeds()
                   from prob in GetFaultProbabilities()
                   from hosts in GetHostCounts()
                   from clients in GetClientCounts()
                   from steps in GetStepCounts()
                   from isMut in GetIsMutated()

                   where !(hosts == 1 && clients >= 6)
                   where !(clients <= 2 && steps >= 10)
                   select new TestCaseData(seed, prob, hosts, clients, steps, isMut);
        }

        /// <summary>
        /// The seeds to use
        /// </summary>
        private IEnumerable<int> GetSeeds()
        {
            //yield return 0xE99032B;
            //yield return 0x4F009539;
            //yield return 0x319140E0;
            yield return 0x36159C73;
            yield return 0x60E70223;
        }

        /// <summary>
        /// The general fault injection/repair probabilities to use in test cases
        /// </summary>
        private IEnumerable<double> GetFaultProbabilities()
        {
            //yield return 0.0;
            yield return 0.3;
        }

        /// <summary>
        /// The hosts counts to use in test cases
        /// </summary>
        private IEnumerable<int> GetHostCounts()
        {
            yield return 1;
            yield return 2;
        }

        /// <summary>
        /// The client counts to use in test cases
        /// </summary>
        private IEnumerable<int> GetClientCounts()
        {
            //yield return 1;
            yield return 2;
            yield return 4;
            yield return 6;
        }

        /// <summary>
        /// The step counts to use in test cases
        /// </summary>
        private IEnumerable<int> GetStepCounts()
        {
            yield return 5;
            yield return 10;
            //yield return 12;
        }

        /// <summary>
        /// Indicates if the cluster is mutated for the test cases
        /// </summary>
        private IEnumerable<bool> GetIsMutated()
        {
            yield return false;
            yield return true;
        }

        #endregion

        #region Execution

        /// <summary>
        /// Executing the case study
        /// </summary>
        /// <param name="benchmarkSeed">The benchmark seed</param>
        /// <param name="faultProbability">The base probability for faults</param>
        /// <param name="hostsCount">The hosts count</param>
        /// <param name="clientCount">The client count</param>
        /// <param name="stepCount">The step count</param>
        /// <param name="isMutated">Using the mutated cluster scenario</param>
        /// <remarks>
        /// To execute a test case, all physical hosts must be reachable via SSH and
        /// the connection and login data written to <see cref="ModelSettings.SshHosts"/>,
        /// <see cref="ModelSettings.SshUsernames"/> and <see cref="ModelSettings.SshPrivateKeyFiles"/>.
        /// It will create the needed connector instances before the first test execution.
        /// The cluster will be started before the test and stopped after the test.
        /// </remarks>
        [Test]
        [TestCaseSource(nameof(GetTestCases))]
        public void ExecuteCaseStudy(int benchmarkSeed, double faultProbability, int hostsCount,
                                     int clientCount, int stepCount, bool isMutated)
        {
            Logger.Info("Starting Case Study test");
            Logger.Info("Parameter:");
            Logger.Info($"  benchmarkSeed=    0x{benchmarkSeed:X8} ({benchmarkSeed:D})");
            Logger.Info($"  faultProbability= {faultProbability}");
            Logger.Info($"  hostsCount=       {hostsCount}");
            Logger.Info($"  clientCount=      {clientCount}");
            Logger.Info($"  stepCount=        {stepCount}");
            Logger.Info($"  isMutated=        {isMutated}");

            InitInstances();
            var isFailed = false;
            try
            {
                // Setup
                StartCluster(hostsCount, isMutated);
                Thread.Sleep(5000); // wait for startup

                Logger.Info("Setting up test case");
                var simTest = new SimulationTests
                {
                    IsRestartingNodesAfterFaultingSimulation = false,
                    MinStepTime = new TimeSpan(0, 0, 0, 25),
                    RecreatePreInputs = true,
                    PrecreatedInputs = false,
                    NodeBaseCount = 4,

                    BenchmarkSeed = benchmarkSeed,
                    FaultActivationProbability = faultProbability,
                    FaultRepairProbability = faultProbability,
                    HostsCount = hostsCount,
                    ClientCount = clientCount,

                    StepCount = stepCount,
                };

                // Execution
                simTest.SimulateHadoopFaults();
            }
            catch(Exception e)
            {
                Logger.Fatal("Fatal exception during executing test case.", e);
                isFailed = true;
            }
            finally
            {
                // Teardown
                StopCluster();
                MoveCaseStudyLogs(benchmarkSeed, faultProbability, hostsCount,
                    clientCount, stepCount, isMutated);
            }

            Assert.False(isFailed);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Resets needed instances and if not already initialized, initializes the connector instances
        /// </summary>
        private void InitInstances()
        {
            Model.ResetInstance();
            OutputUtilities.Reset();

            if(IsInitConnectors)
                return;

            ModelSettings.HostMode = ModelSettings.EHostMode.Multihost;
            ModelSettings.HostsCount = GetHostCounts().Max();
            var cmd = CmdConnector.Instance;
            var rest = RestConnector.Instance;
            IsInitConnectors = cmd != null && rest != null;
        }

        /// <summary>
        /// Starts the Cluster for the case study
        /// </summary>
        /// <param name="hostsCount">The hosts count</param>
        /// <param name="isMutated">True if the mutated scenario should be started</param>
        private void StartCluster(int hostsCount, bool isMutated)
        {
            Logger.Info($"Start cluster on {hostsCount} hosts (mutated: {isMutated})");
            ModelSettings.HostMode = ModelSettings.EHostMode.Multihost;
            ModelSettings.HostsCount = hostsCount;

            var config = isMutated ? MutationConfig : String.Empty;

            var isStarted = CmdConnector.Instance.StartCluster(config);
            Logger.Info($"Is cluster started: {isStarted}");
            Assert.IsTrue(isStarted, $"failed to start cluster on {hostsCount} hosts (mutated: {isMutated})");
        }

        /// <summary>
        /// Stops the cluster
        /// </summary>
        private void StopCluster()
        {
            Logger.Info("Stop cluster");
            var isStopped = CmdConnector.Instance.StopCluster();
            Logger.Info($"Is cluster stopped: {isStopped}");
            Assert.IsTrue(isStopped, "failed to stop cluster)");
        }

        /// <summary>
        /// Moving the case study log files so each case study has its own logs.
        /// Logs will be moved from /logs to /testingHadoopCaseStudyLogs with
        /// following filename scheme:
        /// "<see cref="benchmarkSeed"/>-<see cref="faultProbability"/>-
        /// <see cref="hostsCount"/>-<see cref="clientCount"/>-
        /// <see cref="stepCount"/>-<see cref="isMutated"/>-today"
        /// where today is in format "yyMMdd".
        /// </summary>
        /// <param name="benchmarkSeed">The benchmark seed</param>
        /// <param name="faultProbability">The base probability for faults</param>
        /// <param name="hostsCount">The hosts count</param>
        /// <param name="clientCount">The client count</param>
        /// <param name="stepCount">The step count</param>
        /// <param name="isMutated">Using the mutated cluster scenario</param>
        private void MoveCaseStudyLogs(int benchmarkSeed, double faultProbability, int hostsCount,
                                       int clientCount, int stepCount, bool isMutated)
        {
            var origLogDir = $@"{Environment.CurrentDirectory}\logs";
            var todayStrLong = DateTime.Today.ToString("yyyy-MM-dd");
            var origLogFile = $@"{origLogDir}\{todayStrLong}.log";
            var origSshLog = $@"{origLogDir}\{todayStrLong}-sshout.log";

            var caseStudyLogDir = $@"{Environment.CurrentDirectory}\testingHadoopCaseStudyLogs";
            var todayStrShort = DateTime.Today.ToString("yyMMdd");
            var mutated = isMutated ? "MT" : "MF";
            var faultProbStr = faultProbability.ToString(CultureInfo.InvariantCulture);
            var baseFileName = $"0x{benchmarkSeed:X8}-{faultProbStr}F-{hostsCount:D1}H-" +
                           $"{clientCount:D1}C-{stepCount:D2}S-{mutated}-{todayStrShort}";
            var newLogFile = $@"{caseStudyLogDir}\{baseFileName}.log";
            var newSshLog = $@"{caseStudyLogDir}\{baseFileName}-ssh.log";

            Directory.CreateDirectory(caseStudyLogDir);
            File.Move(origLogFile, newLogFile);
            File.Move(origSshLog, newSshLog);
        }

        #endregion

    }
}