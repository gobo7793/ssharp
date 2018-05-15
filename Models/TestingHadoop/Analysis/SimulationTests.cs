﻿#region License
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
using System.Threading;
using NUnit.Framework;
using SafetySharp.Analysis;
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Analysis
{
    public class SimulationTests
    {
        private static log4net.ILog Logger { get; } =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Settings

        // Simulation settings
        private static readonly TimeSpan _MinStepTime = new TimeSpan(0, 0, 0, 20);
        //private static readonly int _BenchmarkSeed = 1;
        private static readonly int _BenchmarkSeed = Environment.TickCount;
        private static readonly int _StepCount = 3;
        private static readonly bool _PrecreatedInputs = true;
        private static readonly double _FaultActivationProbability = 95;
        private static readonly int _HostsCount = 1;
        private static readonly int _NodeBaseCount = 4;

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
            var benchController = new BenchmarkController(_BenchmarkSeed);
            Logger.Info($"Simulating Benchmarks with Seed {_BenchmarkSeed}:");
            for(int i = 0; i < _StepCount; i++)
            {
                benchController.ChangeBenchmark();
                Logger.Info($"Step {i}: {benchController.CurrentBenchmark.Name}");
            }
        }

        /// <summary>
        /// Saves the settings and initializes the model and returns the instance.
        /// </summary>
        /// <returns>The initialized model</returns>
        public Model InitModel()
        {
            ModelSettings.HostMode = ModelSettings.EHostMode.Multihost;
            ModelSettings.HostsCount = _HostsCount;
            ModelSettings.NodeBaseCount = _NodeBaseCount;
            ModelSettings.IsPrecreateBenchInputs = _PrecreatedInputs;

            var model = Model.Instance;
            model.InitModel(appCount: _StepCount, benchTransitionSeed: _BenchmarkSeed);

            return model;
        }

        #endregion

        #region Simulation

        /// <summary>
        /// Simulation without faults
        /// </summary>
        [Test]
        public void SimulateHadoop()
        {
            var origModel = InitModel();
            origModel.Faults.SuppressActivations();

            var wasFatalError = false;
            try
            {
                var simulator = new SafetySharpSimulator(origModel);

                OutputUtilities.PrintExecutionStart();
                OutputUtilities.PrintTestSettings("Simulation", _BenchmarkSeed, _MinStepTime, _StepCount, _PrecreatedInputs);

                SimulateBenchmarks();

                for(var i = 0; i < _StepCount; i++)
                {
                    OutputUtilities.PrintStepStart();
                    var stepStartTime = DateTime.Now;

                    simulator.SimulateStep();

                    var stepTime = DateTime.Now - stepStartTime;
                    OutputUtilities.PrintSteptTime(stepTime);
                    if(stepTime < ModelSettings.MinStepTime)
                        Thread.Sleep(ModelSettings.MinStepTime - stepTime);

                    OutputUtilities.PrintFullTrace(((Model)simulator.Model).Controller);
                }

                OutputUtilities.PrintExecutionFinish();
            }
            catch(Exception e)
            {
                Logger.Fatal("Fatal exception during Faulting Simulation.", e);

                foreach(var node in origModel.Nodes)
                {
                    Model.UsingFaultingConnector.StartNode(node.Name);
                    Model.UsingFaultingConnector.StartNodeNetConnection(node.Name);
                }

                wasFatalError = true;
            }

            Assert.IsFalse(wasFatalError, "fatal error occured, see log for details");
        }

        #endregion

        #region Analysis

        /// <summary>
        /// Simulation without faults
        /// </summary>
        [Test]
        public void SimulateHadoopFaults()
        {
            var origModel = InitModel();
            origModel.Faults.SuppressActivations();

            var wasFatalError = false;
            try
            {
                var simulator = new SafetySharpSimulator(origModel);

                OutputUtilities.PrintExecutionStart();
                OutputUtilities.PrintTestSettings("Simulation", _BenchmarkSeed, _MinStepTime, _StepCount, _PrecreatedInputs);

                SimulateBenchmarks();

                for(var i = 0; i < _StepCount; i++)
                {
                    OutputUtilities.PrintStepStart();
                    var stepStartTime = DateTime.Now;

                    simulator.SimulateStep();

                    var stepTime = DateTime.Now - stepStartTime;
                    OutputUtilities.PrintSteptTime(stepTime);
                    if(stepTime < ModelSettings.MinStepTime)
                        Thread.Sleep(ModelSettings.MinStepTime - stepTime);

                    OutputUtilities.PrintFullTrace(((Model)simulator.Model).Controller);
                }

                OutputUtilities.PrintExecutionFinish();
            }
            catch(Exception e)
            {
                Logger.Fatal("Fatal exception during Faulting Simulation.", e);

                wasFatalError = true;
            }
            finally
            {
                foreach(var node in origModel.Nodes)
                {
                    Model.UsingFaultingConnector.StartNode(node.Name);
                    Model.UsingFaultingConnector.StartNodeNetConnection(node.Name);
                }
            }

            Assert.IsFalse(wasFatalError, "fatal error occured, see log for details");
        }

        ///// <summary>
        ///// Simulation with nondeterministic fault activation using <see cref="SafetySharpProbabilisticSimulator"/>
        ///// </summary>
        //[Test]
        //public void SimulateHadoopProbabilisticSimulator()
        //{
        //    var origModel = InitModel();
        //    foreach(var f in origModel.Faults)
        //        f.ProbabilityOfOccurrence = new ISSE.SafetyChecking.Modeling.Probability(_FaultActivationProbability);
        //    origModel.Faults.MakeNondeterministic();

        //    SimulateBenchmarks();

        //    var wasFatalError = false;
        //    try
        //    {
        //        var simulator = new SafetySharpProbabilisticSimulator(origModel);

        //        OutputUtilities.PrintExecutionStart();
        //        OutputUtilities.PrintTestSettings("Probabilistic Simulation", _BenchmarkSeed, _MinStepTime, _StepCount, _PrecreatedInputs);

        //        simulator.SimulateSteps(_StepCount);

        //        OutputUtilities.PrintExecutionFinish();
        //    }
        //    catch(Exception e)
        //    {
        //        Logger.Fatal("Fatal exception during Faulting Simulation.", e);

        //        wasFatalError = true;
        //    }
        //    finally
        //    {
        //        Logger.Info("Restarting all Nodes...");
        //        foreach(var node in origModel.Nodes)
        //        {
        //            Model.UsingFaultingConnector.StopNode(node.Name);
        //            Model.UsingFaultingConnector.StartNode(node.Name);
        //        }
        //    }

        //    Assert.IsFalse(wasFatalError, "fatal error occured, see log for details");
        //}

        #endregion
    }
}