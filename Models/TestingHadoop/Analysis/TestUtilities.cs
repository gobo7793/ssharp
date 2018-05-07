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
using SafetySharp.CaseStudies.TestingHadoop.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Analysis
{
    /// <summary>
    /// General utilities for execution (analysis) tests
    /// </summary>
    public class TestUtilities
    {
        private static log4net.ILog Logger { get; } = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Prints the initial test settings to logging outputs if set
        /// </summary>
        /// <param name="type">Type of the executing test</param>
        /// <param name="benchmarkSeed">The used benchmark seed</param>
        /// <param name="minStepTime">Minimum step time</param>
        /// <param name="stepCount">Executing step count</param>
        /// <param name="isInputsPrecreated">Indicates if the input data of some benchmarks are created before execution</param>
        /// <param name="isFaultActivationEnabled">Indicates if S# fault activation is active</param>
        public static void PrintTestSettings(string type, int? benchmarkSeed = null, TimeSpan? minStepTime = null, int? stepCount = null,
                                             bool? isInputsPrecreated = null, bool? isFaultActivationEnabled = null)
        {
            Logger.Info($"Starting {type} test");

            if(benchmarkSeed.HasValue)
                Logger.Info($"Benchmark seed:    {benchmarkSeed}");
            if(minStepTime.HasValue)
                Logger.Info($"Min Step time:     {minStepTime}");
            if(stepCount.HasValue)
                Logger.Info($"Step count:        {stepCount}");
            if(isFaultActivationEnabled.HasValue)
                Logger.Info($"Fault activation:  {stepCount}");
            if(isInputsPrecreated.HasValue)
                Logger.Info($"Inputs precreated: {isInputsPrecreated}");

            Logger.Info($"Host mode:      {Model.HostMode}");
            Logger.Info($"Setup script:   {Model.HadoopSetupScript}");
            Logger.Info($"Controller url: {Model.ControllerRestRmUrl}");
        }

        /// <summary>
        /// Prints the current model trace to logging outputs
        /// </summary>
        /// <param name="model">Model to print</param>
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