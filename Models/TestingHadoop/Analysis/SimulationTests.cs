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

    public class SimulationTests
    {
        private static log4net.ILog Logger { get; } =
            log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Settings

        // Simulation settings
        private static readonly TimeSpan _MinStepTime = new TimeSpan(0, 0, 0, 25);
        //private static readonly int _BenchmarkSeed = 1;
        //private static readonly int _BenchmarkSeed = Environment.TickCount;
        private static readonly int _BenchmarkSeed = 4770109;
        private static readonly int _StepCount = 3;
        private static readonly bool _PrecreatedInputs = true;
        private static readonly double _FaultActivationProbability = 0.4; // 0.0 -> inactive, 1.0 -> always
        private static readonly double _FaultRepairProbability = 0.5; // 0.0 -> inactive, 1.0 -> always
        private static readonly int _HostsCount = 1;
        private static readonly int _NodeBaseCount = 4;
        private static readonly int _ClientCount = 1;

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

        #endregion

        #region Simulation

        private Tuple<int?, int?> _FaultCounts { get; set; }

        /// <summary>
        /// Simulation without faults
        /// </summary>
        [Test]
        public void SimulateHadoop()
        {
            ModelSettings.FaultActivationProbability = 0.0;
            ModelSettings.FaultRepairProbability = 1.0;

            ExecuteSimulation(false);
        }

        /// <summary>
        /// Simulation with faults
        /// </summary>
        [Test]
        public void SimulateHadoopFaults()
        {
            ModelSettings.FaultActivationProbability = _FaultActivationProbability;
            ModelSettings.FaultRepairProbability = _FaultRepairProbability;

            ExecuteSimulation(true);
        }

        /// <summary>
        /// Execution of the simulation
        /// </summary>
        /// <param name="isWithFaults">Indicates if the fault activation mechanism is active</param>
        private void ExecuteSimulation(bool isWithFaults)
        {
            var model = InitModel();

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
                    _FaultCounts = CountFaults(faults);
                    Oracle.ValidateConstraints("simulator", TestConstraints);
                }

                OutputUtilities.PrintTestResults(_FaultCounts?.Item1, _FaultCounts?.Item2);

                OutputUtilities.PrintExecutionFinish();
            }
            catch(Exception e)
            {
                Logger.Fatal("Fatal exception during Faulting Simulation.", e);

                wasFatalError = true;
            }
            finally
            {
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
                if(_FaultCounts == null)
                    return true;
                if(_FaultCounts.Item1.HasValue && _FaultCounts.Item1.Value > 0)
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