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

        // Simulation settings
        private static readonly TimeSpan _MinStepTime = new TimeSpan(0, 0, 0, 20);

        //private static readonly int _BenchmarkSeed = 1;
        private static readonly int _BenchmarkSeed = Environment.TickCount;
        private static readonly int _StepCount = 3;
        private static readonly bool _PrecreatedInputs = false;
        private static readonly double _FaultActivationProbability = 0.000001;

        /// <summary>
        /// Only create input data for other benchmarks without simulation
        /// </summary>
        [Test]
        public void PrecreateInputData()
        {
            BenchmarkController.PrecreateInputData();
        }

        /// <summary>
        /// Simulation without faults
        /// </summary>
        [Test]
        public void SimulateHadoop()
        {
            Model.HostMode = Model.EHostMode.Multihost;
            Model.IsPrecreateBenchInputs = _PrecreatedInputs;
            var origModel = Model.Instance;
            origModel.InitModel(appCount: _StepCount, benchTransitionSeed: _BenchmarkSeed);
            origModel.Faults.SuppressActivations();

            try
            {
                var simulator = new SafetySharpSimulator(origModel);
                var model = (Model)simulator.Model;

                OutputUtilities.PrintExecutionStart();
                OutputUtilities.PrintTestSettings("Simulation", _BenchmarkSeed, _MinStepTime, _StepCount, _PrecreatedInputs);

                for(var i = 0; i < _StepCount; i++)
                {
                    simulator.SimulateStep();
                }

                OutputUtilities.PrintExecutionFinish();
            }
            catch(Exception e)
            {
                Logger.Fatal("Fatal exception during Simulation.", e);
                Assert.Fail("See logging output");
            }
        }

        /// <summary>
        /// Simulation with nondeterministic fault activation using <see cref="SafetySharpProbabilisticSimulator"/>
        /// </summary>
        [Test]
        public void SimulateHadoopFaults()
        {
            Model.HostMode = Model.EHostMode.Multihost;
            Model.IsPrecreateBenchInputs = _PrecreatedInputs;
            var origModel = Model.Instance;
            origModel.InitModel(appCount: _StepCount, benchTransitionSeed: _BenchmarkSeed);
            foreach(var f in origModel.Faults)
                f.ProbabilityOfOccurrence = new ISSE.SafetyChecking.Modeling.Probability(_FaultActivationProbability);
            origModel.Faults.MakeNondeterministic();

            try
            {
                var simulator = new SafetySharpProbabilisticSimulator(origModel);
                var model = (Model)simulator.Model;

                OutputUtilities.PrintExecutionStart();
                OutputUtilities.PrintTestSettings("Probabilistic Simulation", _BenchmarkSeed, _MinStepTime, _StepCount, _PrecreatedInputs);

                simulator.SimulateSteps(_StepCount);

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

                Assert.Fail("See logging output");
            }
        }

    }
}