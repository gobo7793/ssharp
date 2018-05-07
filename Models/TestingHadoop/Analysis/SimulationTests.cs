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
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Analysis
{
    public class SimulationTests
    {
        private static log4net.ILog Logger { get; } = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Simulation settings
        private static readonly TimeSpan _MinStepTime = new TimeSpan(0, 0, 0, 20);
        //private static readonly int _BenchmarkSeed = 1;
        private static readonly int _BenchmarkSeed = Environment.TickCount;
        private static readonly int _StepCount = 10;
        private static readonly bool _PrecreatedInputs = true;
        private static readonly bool _IsFaultActivationActive = true;

        [Test]
        public void Simulate()
        {
            Model.HostMode = Model.EHostMode.Multihost;
            Model.IsPrecreateBenchInputs = _PrecreatedInputs;
            var model = Model.Instance;
            model.InitModel(appCount: _StepCount, benchTransitionSeed: _BenchmarkSeed);
            if(!_IsFaultActivationActive)
                model.Faults.SuppressActivations();
            else
                model.Faults.MakeNondeterministic();

            try
            {
                var simulator = new SafetySharpSimulator(model);
                ExecuteSimulation(simulator, _StepCount);
            }
            catch(Exception e)
            {
                Logger.Fatal("Fatal exception during simulation.", e);
            }
        }

        public static void ExecuteSimulation(SafetySharpSimulator simulator, int steps)
        {
            var model = (Model)simulator.Model;

            Logger.Info("=================  START  =====================================");
            Logger.Info($"Benchmark seed: {_BenchmarkSeed}");
            Logger.Info($"Min Step time:  {_MinStepTime}");
            Logger.Info($"Step count:     {_StepCount}");
            Logger.Info($"Host mode:      {Model.HostMode}");
            Logger.Info($"Setup script:   {Model.HadoopSetupScript}");
            Logger.Info($"Controller url: {Model.ControllerRestRmUrl}");

            for(var i = 0; i < steps; i++)
            {
                Logger.Info($"=================  Step: {i}  =====================================");
                var stepStartTime = DateTime.Now;

                simulator.SimulateStep();

                var stepTime = DateTime.Now - stepStartTime;
                if(stepTime < _MinStepTime)
                    Thread.Sleep(_MinStepTime - stepTime);

                Logger.Info($"Duration: {stepTime.ToString()}");

                PrintTrace(model);
            }

            Logger.Info("=================  Finish  =====================================");
        }

        public static void PrintTrace(Model model)
        {
            foreach(var node in model.Controller.ConnectedNodes)
            {
                Logger.Info($"=== Node {node.NodeId} ===");
                Logger.Info($"    State:       {node.State}");
                Logger.Info($"    IsActive:    {node.IsActive}");
                Logger.Info($"    IsConnected: {node.IsConnected}");
            }

            foreach(var client in model.Controller.ConnectedClients)
            {
                Logger.Info($"=== Client {client.ClientDir} ===");
                Logger.Info($"    Current executing bench: {client.BenchController?.CurrentBenchmark?.Name}");
                Logger.Info($"    Current executing app:   {client.CurrentExecutingApp?.AppId}");

                foreach(var app in client.Apps)
                {
                    if(String.IsNullOrWhiteSpace(app.AppId))
                        continue;

                    Logger.Info($"    === App {app.AppId} ===");
                    Logger.Info($"        Name:        {app.Name}");
                    Logger.Info($"        State:       {app.State}");
                    Logger.Info($"        FinalStatus: {app.FinalStatus}");
                    Logger.Info($"        AM Host:     {app.AmHostId} ({app.AmHost?.State})");

                    foreach(var attempt in app.Attempts)
                    {
                        if(String.IsNullOrWhiteSpace(attempt.AttemptId))
                            continue;

                        Logger.Info($"        === Attempt {attempt.AttemptId} ===");
                        Logger.Info($"            State:        {attempt.State}");
                        Logger.Info($"            AM Container: {attempt.AmContainerId}");
                        Logger.Info($"            AM Host:      {attempt.AmHostId} ({attempt.AmHost?.State})");

                        foreach(var container in attempt.Containers)
                        {
                            if(String.IsNullOrWhiteSpace(container.ContainerId))
                                continue;

                            Logger.Info($"          === Container {container.ContainerId} ===");
                            Logger.Info($"              State: {container.State}");
                            Logger.Info($"              Host:  {container.HostId} ({container.Host?.State})");
                        }
                    }
                }
            }
        }
    }
}