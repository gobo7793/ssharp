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
using System.Threading;
using ISSE.SafetyChecking.Formula;
using ISSE.SafetyChecking.MinimalCriticalSetAnalysis;
using NUnit.Framework;
using SafetySharp.Analysis;
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.ModelChecking;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Analysis
{
    public class AnalysisTests
    {
        private static log4net.ILog Logger { get; } = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Simulation settings
        private static readonly TimeSpan _MinStepTime = new TimeSpan(0, 0, 0, 20);
        private static readonly int _BenchmarkSeed = Environment.TickCount;
        private static readonly int _StepCount = 10;
        private static readonly bool _PrecreatedInputs = true;

        /// <summary>
        /// Analyzing model using DCCA (need >12 GB RAM)
        /// </summary>
        [Test]
        public void Analyse()
        {
            Model.HostMode = Model.EHostMode.Multihost;
            Model.IsPrecreateBenchInputs = _PrecreatedInputs;
            var model = Model.Instance;
            model.InitModel(appCount: _StepCount, benchTransitionSeed: _BenchmarkSeed);

            try
            {
                //var simulator = new SafetySharpSimulator(model);
                //ExecuteAnalysis(simulator, _StepCount);
                Formula hazard = model.Nodes.All(n => n.State != Modeling.HadoopModel.ENodeState.RUNNING);
                var result = SafetySharpSafetyAnalysis.AnalyzeHazard(model, hazard);
                Logger.Info(result);
            }
            catch(Exception e)
            {
                Logger.Fatal("Fatal exception during test.", e);
                Assert.Fail("See logging output");
            }
        }

        //public static void ExecuteAnalysis(SafetySharpSimulator simulator, int steps)
        //{
        //    var model = (Model)simulator.Model;

        //    Logger.Info("=================  START  =====================================");
        //    OutputUtilities.PrintTestSettings("DCCA", _BenchmarkSeed, _MinStepTime, _StepCount, _PrecreatedInputs);

        //    for(var i = 0; i < steps; i++)
        //    {
        //        Logger.Info($"=================  Step: {i}  =====================================");
        //        var stepStartTime = DateTime.Now;

        //        simulator.SimulateStep();

        //        var stepTime = DateTime.Now - stepStartTime;
        //        if(stepTime < _MinStepTime)
        //            Thread.Sleep(_MinStepTime - stepTime);

        //        Logger.Info($"Duration: {stepTime.ToString()}");

        //        OutputUtilities.PrintTrace(model);
        //    }

        //    Logger.Info("=================  Finish  =====================================");
        //}

    }
}