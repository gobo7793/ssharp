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
        private static readonly TimeSpan _StepMinTime = new TimeSpan(0, 0, 0, 30);
        private static readonly int _StepCount = 10;
        private static readonly Logger.Level _LogLevel = Logger.Level.Log;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            Logger.LogLevel = _LogLevel;
        }

        [Test]
        public void Simulate()
        {
            var model = Model.Instance;
            model.InitModel(appCount: _StepCount);
            model.Faults.SuppressActivations();

            try
            {
                var simulator = new SafetySharpSimulator(model);
                ExecuteSimulation(simulator, _StepCount);
            }
            catch(Exception e)
            {
                Logger.Exception(e.ToString());
            }
        }

        public static void ExecuteSimulation(SafetySharpSimulator simulator, int steps)
        {
            var model = (Model)simulator.Model;

            Logger.Log("=================  START  =====================================");

            for(var i = 0; i < steps; i++)
            {
                Logger.Log($"=================  Step: {i}  =====================================");
                var stepStartTime = DateTime.Now;

                simulator.SimulateStep();

                var stepTime = DateTime.Now - stepStartTime;
                //if(stepTime < _StepMinTime)
                //    Thread.Sleep(_StepMinTime - stepTime);

                Logger.Log($"Duration: {stepTime.ToString()}");

                PrintTrace(model);
            }

            Logger.Log("=================  Finish  =====================================");
            Logger.Log();
        }

        public static void PrintTrace(Model model)
        {
            foreach(var node in model.Controller.ConnectedNodes)
            {
                Logger.Log($"=== Node {node.NodeId} ===");
                Logger.Log($"    State:       {node.State}");
                Logger.Log($"    IsActive:    {node.IsActive}");
                Logger.Log($"    IsConnected: {node.IsConnected}");
            }

            foreach(var client in model.Controller.ConnectedClients)
            {
                Logger.Log($"=== Client {client.ClientDir} ===");
                Logger.Log($"    Current executing bench: {client.BenchController?.CurrentBenchmark?.Name}");
                Logger.Log($"    Current executing app:   {client.CurrentExecutingApp?.AppId}");

                foreach(var app in client.Apps)
                {
                    if(String.IsNullOrWhiteSpace(app.AppId))
                        continue;

                    Logger.Log($"  === App {app.AppId} ===");
                    Logger.Log($"      Name:        {app.Name}");
                    Logger.Log($"      State:       {app.State}");
                    Logger.Log($"      FinalStatus: {app.FinalStatus}");
                    Logger.Log($"      AM Host:     {app.AmHostId} ({app.AmHost?.State})");

                    foreach(var attempt in app.Attempts)
                    {
                        if(String.IsNullOrWhiteSpace(attempt.AttemptId))
                            continue;

                        Logger.Log($"    === Attempt {attempt.AttemptId} ===");
                        Logger.Log($"        State:        {attempt.State}");
                        Logger.Log($"        AM Container: {attempt.AmContainerId}");
                        Logger.Log($"        AM Host:      {attempt.AmHostId} ({attempt.AmHost?.State})");

                        foreach(var container in attempt.Containers)
                        {
                            if(String.IsNullOrWhiteSpace(container.ContainerId))
                                continue;

                            Logger.Log($"      === Container {container.ContainerId} ===");
                            Logger.Log($"          State: {container.State}");
                            Logger.Log($"          Host:  {container.HostId} ({container.Host?.State})");
                        }
                    }
                }
            }
        }
    }
}