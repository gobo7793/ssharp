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
using System.Diagnostics;
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

        [Test]
        public void Simulate()
        {
            var model = Model.Instance;
            model.InitModel();
            model.Faults.SuppressActivations();

            var simulator = new SafetySharpSimulator(model);
            PrintTrace(simulator, steps: 1);
        }

        public static void PrintTrace(SafetySharpSimulator simulator, int steps)
        {
            var model = (Model)simulator.Model;

            for(var i = 0; i < steps; i++)
            {
                var stepStartTime = DateTime.Now;

                simulator.SimulateStep();

                var stepTime = DateTime.Now - stepStartTime;
                if(stepTime < _StepMinTime)
                    Thread.Sleep(_StepMinTime - stepTime);

                WriteLine($"=================  Step: {i}  =====================================");
                WriteLine($"Duration: {stepTime.ToString()}");

                PrintTrace(model);
            }

            WriteLine("=================   Finish    =====================================");
        }

        public static void PrintTrace(Model model)
        {
            for(int c = 0; c < model.Clients.Count; c++)
            {
                var client = model.Clients[c];

                WriteLine($"=== Client {c} ===");
                WriteLine($"Current executing bench: {client.BenchController?.CurrentBenchmark?.Name}");
                WriteLine($"Current executing app:   {client.CurrentExecutingApp?.AppId}");

                foreach(var app in model.Clients[c].Apps)
                {
                    if(String.IsNullOrWhiteSpace(app.AppId))
                        continue;

                    WriteLine($"    === App {app.AppId} ===");
                    WriteLine($"    State:       {app.State}");
                    WriteLine($"    FinalStatus: {app.FinalStatus}");
                    WriteLine($"    AM Host:     {app.AmHost?.Name}");

                    foreach(var attempt in app.Attempts)
                    {
                        if(String.IsNullOrWhiteSpace(attempt.AttemptId))
                            continue;

                        WriteLine($"        === Attempt {attempt.AttemptId} ===");
                        WriteLine($"        State:        {attempt.State}");
                        WriteLine($"        AM Container: {attempt.AmContainerId}");
                        WriteLine($"        AM Host:      {attempt.AmHost?.Name}");

                        foreach(var container in attempt.Containers)
                        {
                            if(String.IsNullOrWhiteSpace(container.ContainerId))
                                continue;

                            WriteLine($"            === Container {container.ContainerId} ===");
                            WriteLine($"            State: {container.State}");
                            WriteLine($"            Host:  {container.Host?.Name}");
                        }
                    }
                }
            }

            foreach(var node in model.Nodes)
            {
                WriteLine($"=== Node {node.NodeId} ===");
                WriteLine($"State:       {node.State}");
                WriteLine($"IsActive:    {node.IsActive}");
                WriteLine($"IsConnected: {node.IsConnected}");
            }
        }

        private static void WriteLine(object line)
        {
            Debug.WriteLine(line.ToString());
#if !DEBUG
            Console.WriteLine(line.ToString());
#endif
        }
    }
}