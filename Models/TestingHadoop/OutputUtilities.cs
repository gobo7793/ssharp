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
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop
{
    /// <summary>
    /// General utilities for execution (analysis) tests
    /// </summary>
    public static class OutputUtilities
    {
        private static log4net.ILog Logger { get; } =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static int _StepCount = 0;

        #region General output

        /// <summary>
        /// Prints the initial test settings to logging outputs if set
        /// </summary>
        /// <param name="type">Type of the executing test</param>
        /// <param name="minStepTime">Minimum step time</param>
        /// <param name="stepCount">Executing step count</param>
        public static void PrintTestSettings(string type, TimeSpan? minStepTime = null, int? stepCount = null)
        {
            Logger.Info($"Starting {type} test");

            Logger.Info($"Base benchmark seed: {ModelSettings.RandomBaseSeed}");
            if(minStepTime.HasValue)
                Logger.Info($"Min Step time:       {minStepTime}");
            if(stepCount.HasValue)
                Logger.Info($"Step count:          {stepCount}");

            Logger.Info($"Fault probability:   {ModelSettings.FaultActivationProbability}");
            Logger.Info($"Fault repair prob.:  {ModelSettings.FaultRepairProbability}");
            Logger.Info($"Inputs precreated:   {ModelSettings.IsPrecreateBenchInputs}");

            Logger.Info($"Host mode:           {ModelSettings.HostMode}");
            Logger.Info($"Hosts couent:        {ModelSettings.HostsCount}");
            Logger.Info($"Node base count:     {ModelSettings.NodeBaseCount}");
            Logger.Info($"Setup script path:   {ModelSettings.HadoopSetupScript}");
            Logger.Info($"Controller url:      {ModelSettings.ControllerRestRmUrl}");
        }

        /// <summary>
        /// Prints the start of the simulation/execution
        /// </summary>
        public static void PrintExecutionStart()
        {
            Logger.Info("=================  START  =====================================");
        }

        /// <summary>
        /// Prints the start of a simulation/execution step
        /// </summary>
        public static void PrintStepStart()
        {
            PrintStepStart(_StepCount++);
        }

        /// <summary>
        /// Prints the start of the given simulation/execution step
        /// </summary>
        /// <param name="stepCount">The step counter for the current step</param>
        public static void PrintStepStart(int stepCount)
        {
            Logger.Info($"=================  Step: {stepCount}  =====================================");
        }

        /// <summary>
        /// Prints the time needed to execute the step
        /// </summary>
        /// <param name="stepTime">The execution time</param>
        public static void PrintSteptTime(TimeSpan stepTime)
        {
            Logger.Info($"Duration: {stepTime.ToString()}");
        }

        /// <summary>
        /// Prints the end of the simulation/execution
        /// </summary>
        public static void PrintExecutionFinish()
        {
            Logger.Info("=================  Finish  =====================================");
        }

        #endregion

        #region Trace output

        /// <summary>
        /// Prints the current node trace to logging outputs
        /// </summary>
        /// <param name="node">Node to print</param>
        public static void PrintTrace(YarnNode node)
        {
            Logger.Info($"=== Node {node.NodeId} ===");
            Logger.Info($"    State:         {node.State}");
            Logger.Info($"    IsActive:      {node.IsActive}");
            Logger.Info($"    IsConnected:   {node.IsConnected}");
            Logger.Info($"    Container Cnt: {node.RunningContainerCount}");
            Logger.Info($"    Mem used/free: {node.MemoryUsed}/{node.MemoryAvailable} ({node.MemoryUsage:F3})");
            Logger.Info($"    CPU used/free: {node.CpuUsed}/{node.CpuAvailable} ({node.MemoryUsage:F3})");
        }

        /// <summary>
        /// Prints the current client trace to logging outputs
        /// </summary>
        /// <param name="client">Client to print</param>
        public static void PrintTrace(Client client)
        {
            Logger.Info($"=== Client {client.ClientDir} ===");
            Logger.Info($"    Current executing bench: {client.BenchController?.CurrentBenchmark?.Name}");
            Logger.Info($"    Current executing app:   {client.CurrentExecutingApp?.AppId}");
        }

        /// <summary>
        /// Prints the current application trace to logging outputs
        /// </summary>
        /// <param name="app">Application to print</param>
        public static void PrintTrace(YarnApp app)
        {
            Logger.Info($"    === App {app.AppId} ===");
            Logger.Info($"        Name:        {app.Name}");
            Logger.Info($"        State:       {app.State}");
            Logger.Info($"        FinalStatus: {app.FinalStatus}");
            Logger.Info($"        AM Host:     {app.AmHostId} ({app.AmHost?.State})");
        }

        /// <summary>
        /// Prints the current application attempt trace to logging outputs
        /// </summary>
        /// <param name="attempt">Attempt to print</param>
        public static void PrintTrace(YarnAppAttempt attempt)
        {
            Logger.Info($"        === Attempt {attempt.AttemptId} ===");
            Logger.Info($"            State:        {attempt.State}");
            Logger.Info($"            AM Container: {attempt.AmContainerId}");
            Logger.Info($"            AM Host:      {attempt.AmHostId} ({attempt.AmHost?.State})");
        }

        /// <summary>
        /// Prints the current container trace to logging outputs
        /// </summary>
        /// <param name="container">Container to print</param>
        public static void PrintTrace(YarnAppContainer container)
        {
            Logger.Info($"          === Container {container.ContainerId} ===");
            Logger.Info($"              State: {container.State}");
            Logger.Info($"              Host:  {container.HostId} ({container.Host?.State})");
        }

        #endregion

        #region Full trace output

        /// <summary>
        /// Prints the current controller trace to logging outputs 
        /// and prints also the node trace of its nodes
        /// and the full client trace of its client
        /// </summary>
        /// <param name="controller">Controller to print</param>
        public static void PrintFullTrace(YarnController controller)
        {
            foreach(var node in controller.ConnectedNodes)
                PrintTrace(node);

            foreach(var client in controller.ConnectedClients)
                PrintFullTrace(client);
        }

        /// <summary>
        /// Prints the current client trace to logging outputs 
        /// and prints also the full application trace of its applications
        /// </summary>
        /// <param name="client">Client to print</param>
        public static void PrintFullTrace(Client client)
        {
            PrintTrace(client);

            foreach(var app in client.Apps)
                PrintFullTrace(app);
        }

        /// <summary>
        /// Prints the current application trace to logging outputs if the application
        /// has an ID and prints also the full attempt trace of its attempts
        /// </summary>
        /// <param name="app">Application to print</param>
        public static void PrintFullTrace(YarnApp app)
        {
            if(String.IsNullOrWhiteSpace(app.AppId))
                return;

            PrintTrace(app);

            foreach(var attempt in app.Attempts)
                PrintFullTrace(attempt);
        }

        /// <summary>
        /// Prints the current application attempt trace to logging outputs if the attempt
        /// has an ID and prints also the container trace of its containers that have an ID
        /// </summary>
        /// <param name="attempt">Attempt to print</param>
        public static void PrintFullTrace(YarnAppAttempt attempt)
        {
            if(String.IsNullOrWhiteSpace(attempt.AttemptId))
                return;

            PrintTrace(attempt);

            foreach(var container in attempt.Containers)
            {
                if(String.IsNullOrWhiteSpace(container.ContainerId))
                    continue;

                PrintTrace(container);
            }
        }

        #endregion
    }
}