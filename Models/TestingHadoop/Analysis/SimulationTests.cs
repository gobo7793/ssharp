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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ISSE.SafetyChecking.Modeling;
using NUnit.Framework;
using SafetySharp.Analysis;
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Analysis
{
    // Fault, Attribute, Node, Activation count, repairing count
    using FaultTuple = Tuple<Fault, NodeFaultAttribute, YarnNode, SimulationTests.IntWrapper, SimulationTests.IntWrapper>;

    [TestFixture]
    public class SimulationTests
    {
        private static log4net.ILog Logger { get; } =
            log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Settings

        // Simulation settings
        private static TimeSpan _MinStepTime = new TimeSpan(0, 0, 0, 25);

        //private static int _BenchmarkSeed = 1;
        private static int _BenchmarkSeed = Environment.TickCount;
        private static int _StepCount = 3;
        private static bool _RecreatePreInputs = false;
        private static bool _PrecreatedInputs = true;
        private static double _FaultActivationProbability = 0.25; // 0.0 -> inactive, 1.0 -> always
        private static double _FaultRepairProbability = 0.5; // 0.0 -> inactive, 1.0 -> always
        private static int _HostsCount = 1;
        private static int _NodeBaseCount = 4;
        private static int _ClientCount = 2;

        #endregion

        #region Preparing

        /// <summary>
        /// Only create input data for other benchmarks without simulation
        /// </summary>
        [Test]
        public void PrecreateInputData()
        {
            BenchmarkController.PrecreateInputData();
        }

        /// <summary>
        /// Recreates input data for other benchmarks without simulation
        /// </summary>
        [Test]
        public void RecreateInputData()
        {
            BenchmarkController.PrecreateInputData(true);
        }

        /// <summary>
        /// Only simulates which benchmarks will be executed
        /// </summary>
        [Test]
        public void SimulateBenchmarks()
        {
            for(int i = 1; i <= _ClientCount; i++)
            {
                var seed = _BenchmarkSeed + i;
                var benchController = new BenchmarkController(seed);
                Logger.Info($"Simulating Benchmarks for Client {i} with Seed {seed}:");
                for(int j = 0; j < _StepCount; j++)
                {
                    benchController.ChangeBenchmark();
                    Logger.Info($"Step {j}: {benchController.CurrentBenchmark.Name}");
                }
            }
        }

        /// <summary>
        /// Saves the settings and initializes the model and returns the instance.
        /// </summary>
        /// <returns>The initialized model</returns>
        private Model InitModel()
        {
            ModelSettings.HostMode = ModelSettings.EHostMode.Multihost;
            ModelSettings.HostsCount = _HostsCount;
            ModelSettings.NodeBaseCount = _NodeBaseCount;
            ModelSettings.IsPrecreateBenchInputsRecreate = _RecreatePreInputs;
            ModelSettings.IsPrecreateBenchInputs = _PrecreatedInputs;
            ModelSettings.RandomBaseSeed = _BenchmarkSeed;

            var model = Model.Instance;
            model.InitModel(appCount: _StepCount, clientCount: _ClientCount);
            model.Faults.SuppressActivations();

            return model;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Collects the faults from <see cref="YarnNode"/>
        /// </summary>
        /// <param name="model">The model</param>
        /// <returns>The Faults and their fault activation attributes</returns>
        private FaultTuple[] CollectYarnNodeFaults(Model model)
        {
            return (from node in model.Nodes

                    from faultField in node.GetType().GetFields()
                    where typeof(Fault).IsAssignableFrom(faultField.FieldType)

                    let attribute = faultField.GetCustomAttribute<NodeFaultAttribute>()
                    where attribute != null

                    let fault = (Fault)faultField.GetValue(node)

                    select Tuple.Create(fault, attribute, node, new IntWrapper(0), new IntWrapper(0))
            ).ToArray();
        }

        /// <summary>
        /// Execute fault handling
        /// </summary>
        /// <param name="faults">The fault tuples</param>
        private void HandleFaults(FaultTuple[] faults)
        {
            foreach(var fault in faults)
            {
                Logger.Info($"Fault {fault.Item1.Name}@{fault.Item3.Name}");
                if(!fault.Item1.IsActivated && fault.Item2.CanActivate(fault.Item3.Name))
                {
                    fault.Item1.ForceActivation();
                    fault.Item4.Value++;
                }
                else if(fault.Item1.IsActivated && fault.Item2.CanRepair())
                {
                    fault.Item1.SuppressActivation();
                    fault.Item5.Value++;
                }
            }
        }

        /// <summary>
        /// Counts the activated and repaired faults
        /// </summary>
        /// <param name="faults">The fault tuple</param>
        /// <returns>The activated and repaired faults count</returns>
        private Tuple<int?, int?> CountFaults(FaultTuple[] faults)
        {
            int? act = 0;
            int? rep = 0;
            foreach(var fault in faults)
            {
                act += fault.Item4.Value;
                rep += fault.Item5.Value;
            }

            return Tuple.Create(act, rep);
        }

        /// <summary>
        /// Moving the case study log files so each case study has its own logs.
        /// Logs will be moved from /logs to /testingHadoopCaseStudyLogs with
        /// following filename scheme:
        /// "<see cref="_BenchmarkSeed"/>-<see cref="_FaultActivationProbability"/>-
        /// <see cref="_HostsCount"/>-<see cref="_ClientCount"/>-<see cref="_StepCount"/>-today"
        /// where today is in format "yyMMdd".
        /// </summary>
        private void MoveCaseStudyLogs()
        {
            var origLogDir = $@"{Environment.CurrentDirectory}\logs";
            var todayStrLong = DateTime.Today.ToString("yyyy-MM-dd");
            var origLogFile = $@"{origLogDir}\{todayStrLong}.log";
            var origSshLog = $@"{origLogDir}\{todayStrLong}-sshout.log";

            var caseStudyLogDir = $@"{Environment.CurrentDirectory}\testingHadoopCaseStudyLogs";
            var todayStrShort = DateTime.Today.ToString("yyMMdd");
            var filename = $"{_BenchmarkSeed:X8}-{_FaultActivationProbability:F1}-" +
                           $"{_HostsCount:D1}-{_ClientCount:D1}-{_StepCount:D2}-{todayStrShort}";
            var newLogFile = $@"{caseStudyLogDir}\{filename}.log";
            var newSshLog = $@"{caseStudyLogDir}\{filename}-ssh.log";

            Directory.CreateDirectory(caseStudyLogDir);
            File.Move(origLogFile, newLogFile);
            File.Move(origSshLog, newSshLog);
        }

        #endregion

        #region Simulation

        /// <summary>
        /// The fault counts (activated/repaired)
        /// </summary>
        private Tuple<int?, int?> FaultCounts { get; set; }

        /// <summary>
        /// Simulation without faults
        /// </summary>
        [Test]
        public void SimulateHadoop()
        {
            ModelSettings.FaultActivationProbability = 0.0;
            ModelSettings.FaultRepairProbability = 1.0;

            ExecuteSimulation();
        }

        /// <summary>
        /// Simulation with faults
        /// </summary>
        [Test]
        public void SimulateHadoopFaults()
        {
            ModelSettings.FaultActivationProbability = _FaultActivationProbability;
            ModelSettings.FaultRepairProbability = _FaultRepairProbability;

            ExecuteSimulation();
        }

        /// <summary>
        /// Execution of the simulation
        /// </summary>
        private void ExecuteSimulation()
        {
            var model = InitModel();
            var isWithFaults = _FaultActivationProbability > 0.000001; // prevent inaccuracy

            var wasFatalError = false;
            try
            {
                // init simulation
                var simulator = new SafetySharpSimulator(model);
                var simModel = (Model)simulator.Model;
                var faults = CollectYarnNodeFaults(simModel);

                OutputUtilities.PrintExecutionStart();
                OutputUtilities.PrintTestSettings("Simulation", _MinStepTime, _StepCount);

                SimulateBenchmarks();

                var simulationStartTime = DateTime.Now;

                // do simuluation
                for(var i = 0; i < _StepCount; i++)
                {
                    OutputUtilities.PrintStepStart();
                    var stepStartTime = DateTime.Now;

                    if(isWithFaults)
                        HandleFaults(faults);
                    simulator.SimulateStep();

                    var stepTime = DateTime.Now - stepStartTime;
                    OutputUtilities.PrintDuration(stepTime);
                    if(stepTime < ModelSettings.MinStepTime)
                        Thread.Sleep(ModelSettings.MinStepTime - stepTime);

                    OutputUtilities.PrintFullTrace(simModel.Controller);
                }

                var simulationTime = DateTime.Now - simulationStartTime;
                OutputUtilities.PrintDuration(simulationTime, "Simulation");

                // collect fault counts and check constraint for it
                if(isWithFaults)
                {
                    FaultCounts = CountFaults(faults);
                    Oracle.ValidateConstraints("simulator", TestConstraints);
                }

                OutputUtilities.PrintTestResults(FaultCounts?.Item1, FaultCounts?.Item2);

                OutputUtilities.PrintExecutionFinish();
            }
            catch(Exception e)
            {
                Logger.Fatal("Fatal exception during Faulting Simulation.", e);

                wasFatalError = true;
            }
            finally
            {
                // kill runnig apps
                Logger.Info("Killing running apps.");
                foreach(var app in model.Applications)
                    app.StopApp();

                // restart nodes
                if(isWithFaults)
                {
                    foreach(var node in model.Nodes)
                    {
                        if(node.IsActive && node.IsConnected)
                            continue;
                        Logger.Info($"Starting node {node.Name} after fault activation.");
                        Model.UsingFaultingConnector.StopNode(node.Name);
                        Model.UsingFaultingConnector.StartNode(node.Name);
                    }
                }
            }

            Assert.IsFalse(wasFatalError, "fatal error occured, see log for details");
        }

        #endregion

        #region Case study test cases

        /// <summary>
        /// Executing the case study
        /// </summary>
        /// <param name="benchmarkSeed">The benchmark seed</param>
        /// <param name="faultProbability">The base probability for faults</param>
        /// <param name="hostsCount">The hosts count</param>
        /// <param name="clientCount">The client count</param>
        /// <param name="stepCount">The step count</param>
        [Test]
        [TestCase(105838460, 0.0, 1, 1, 5)]
        [TestCase(105838460, 0.0, 1, 2, 5)]
        [TestCase(105838460, 0.0, 2, 1, 5)]
        [TestCase(105838460, 0.0, 2, 2, 5)]
        [TestCase(105838460, 0.3, 1, 1, 5)]
        [TestCase(105838460, 0.3, 1, 2, 5)]
        [TestCase(105838460, 0.3, 2, 1, 5)]
        [TestCase(105838460, 0.3, 2, 2, 5)]
        [TestCase(105838460, 0.0, 1, 1, 15)]
        [TestCase(105838460, 0.0, 1, 2, 15)]
        [TestCase(105838460, 0.0, 2, 1, 15)]
        [TestCase(105838460, 0.0, 2, 2, 15)]
        [TestCase(105838460, 0.3, 1, 1, 15)]
        [TestCase(105838460, 0.3, 1, 2, 15)]
        [TestCase(105838460, 0.3, 2, 1, 15)]
        [TestCase(105838460, 0.3, 2, 2, 15)]
        [TestCase(-2044864785, 0.0, 1, 1, 5)]
        [TestCase(-2044864785, 0.0, 1, 2, 5)]
        [TestCase(-2044864785, 0.0, 2, 1, 5)]
        [TestCase(-2044864785, 0.0, 2, 2, 5)]
        [TestCase(-2044864785, 0.3, 1, 1, 5)]
        [TestCase(-2044864785, 0.3, 1, 2, 5)]
        [TestCase(-2044864785, 0.3, 2, 1, 5)]
        [TestCase(-2044864785, 0.3, 2, 2, 5)]
        [TestCase(-2044864785, 0.0, 1, 1, 15)]
        [TestCase(-2044864785, 0.0, 1, 2, 15)]
        [TestCase(-2044864785, 0.0, 2, 1, 15)]
        [TestCase(-2044864785, 0.0, 2, 2, 15)]
        [TestCase(-2044864785, 0.3, 1, 1, 15)]
        [TestCase(-2044864785, 0.3, 1, 2, 15)]
        [TestCase(-2044864785, 0.3, 2, 1, 15)]
        [TestCase(-2044864785, 0.3, 2, 2, 15)]
        [TestCase(514633513, 0.0, 1, 1, 5)]
        [TestCase(514633513, 0.0, 1, 2, 5)]
        [TestCase(514633513, 0.0, 2, 1, 5)]
        [TestCase(514633513, 0.0, 2, 2, 5)]
        [TestCase(514633513, 0.3, 1, 1, 5)]
        [TestCase(514633513, 0.3, 1, 2, 5)]
        [TestCase(514633513, 0.3, 2, 1, 5)]
        [TestCase(514633513, 0.3, 2, 2, 5)]
        [TestCase(514633513, 0.0, 1, 1, 15)]
        [TestCase(514633513, 0.0, 1, 2, 15)]
        [TestCase(514633513, 0.0, 2, 1, 15)]
        [TestCase(514633513, 0.0, 2, 2, 15)]
        [TestCase(514633513, 0.3, 1, 1, 15)]
        [TestCase(514633513, 0.3, 1, 2, 15)]
        [TestCase(514633513, 0.3, 2, 1, 15)]
        [TestCase(514633513, 0.3, 2, 2, 15)]
        public void ExecuteCaseStudy(int benchmarkSeed, double faultProbability, int hostsCount, int clientCount, int stepCount)
        {
            // For all test cases
            _MinStepTime = new TimeSpan(0, 0, 0, 25);
            _RecreatePreInputs = true;
            _PrecreatedInputs = true;
            _NodeBaseCount = 4;

            // Test cases
            _BenchmarkSeed = benchmarkSeed;
            _FaultActivationProbability = faultProbability;
            _FaultRepairProbability = faultProbability;
            _HostsCount = hostsCount;
            _ClientCount = clientCount;

            _StepCount = stepCount;

            // execute
            SimulateHadoopFaults();

            // move logs
            MoveCaseStudyLogs();
        }

        /// <summary>
        /// Generate the case study seeds
        /// </summary>
        [Test]
        public void GenerateCaseStudyBenchSeeds()
        {
            var ticks = Environment.TickCount;
            var ran = new Random(ticks);
            var s1 = ran.Next(int.MinValue, int.MaxValue);
            var s2 = ran.Next(int.MinValue, int.MaxValue);
            var s3 = ran.Next(int.MinValue, int.MaxValue);
            Console.WriteLine($"Ticks: {ticks:X} | s1: {s1:X} | s2: {s2:X} | s3: {s3:X}");
            // Specific output for generating test case seeds:
            // Ticks: 40595187 | s1: 105838460 | s2: -2044864785 | s3: 514633513
        }

        #endregion

        #region Test suite constraint checking related

        /// <summary>
        /// Constraints to check the requirements of the test suite itself
        /// </summary>
        public Func<bool>[] TestConstraints => new Func<bool>[]
        {
            // 7 faults are injected/repaired
            () =>
            {
                OutputUtilities.PrintTestConstraint(7, "simulator");
                if(FaultCounts == null)
                    return true;
                if(FaultCounts.Item1.HasValue && FaultCounts.Item1.Value > 0)
                    return true;
                return false;
            },
        };

        /// <summary>
        /// Wrapper class to change integers inside tuples
        /// </summary>
        internal class IntWrapper
        {
            public int Value { get; set; }

            public IntWrapper(int i)
            {
                Value = i;
            }
        }

        #endregion
    }
}