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

        #region Utilities

        /// <summary>
        /// The step counter for outputting
        /// </summary>
        private static int StepCount { get; set; }

        /// <summary>
        /// Resets the output utilities like the step counter
        /// </summary>
        public static void Reset()
        {
            StepCount = 0;
        }

        #endregion

        #region General test output

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
            Logger.Info($"Hosts count:         {ModelSettings.HostsCount}");
            Logger.Info($"Node base count:     {ModelSettings.NodeBaseCount}");
            Logger.Info($"Full node count:     {ModelUtilities.GetFullNodeCount()}");
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
            PrintStepStart(StepCount);
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
        /// Prints the time needed to execute the simulation part, eg Step or whole simulation
        /// </summary>
        /// <param name="stepTime">The execution time</param>
        /// <param name="type">The simulation part type to print</param>
        public static void PrintDuration(TimeSpan stepTime, string type = "Step")
        {
            Logger.Info($"{type} Duration: {stepTime.ToString()}");
        }

        /// <summary>
        /// Prints the end of a simulation/execution step
        /// </summary>
        public static void PrintStepEnd()
        {
            // nothing to print, but Step++
            StepCount++;
        }

        /// <summary>
        /// Prints the test results
        /// </summary>
        /// <param name="simulationTime">The simulation time</param>
        /// <param name="maxFaultCount">The maximum possible fault activation count</param>
        /// <param name="activatedFaults">Activated fault count</param>
        /// <param name="repairedFaults">Repaired fault count</param>
        public static void PrintTestResults(TimeSpan simulationTime, int? maxFaultCount = null,
                                            int? activatedFaults = null, int? repairedFaults = null)
        {
            Logger.Info("Finishing test.");

            PrintDuration(simulationTime, "Simulation");
            Logger.Info($"Executed Steps:       {StepCount + 1}");
            if(activatedFaults.HasValue)
                Logger.Info($"Activated Faults:     {activatedFaults}/{maxFaultCount ?? 0}");
            if(repairedFaults.HasValue)
                Logger.Info($"Repaired Faults:      {repairedFaults}");
            Logger.Info($"Last detected MARP:   {YarnController.MarpValues.Last(v => v >= 0)}");
            Logger.Info($"Executed apps:        {Model.Instance.Applications.Count(app => !String.IsNullOrWhiteSpace(app.AppId))}");
            Logger.Info($"Successed apps:       {Model.Instance.Applications.Count(app => app.FinalStatus == EFinalStatus.SUCCEEDED)}");
            Logger.Info($"Failed apps:          {Model.Instance.Applications.Count(app => app.FinalStatus == EFinalStatus.FAILED)}");
            Logger.Info($"Killed apps:          {Model.Instance.Applications.Count(app => app.FinalStatus == EFinalStatus.KILLED)}");
            Logger.Info($"Executed attempts:    {Model.Instance.AppAttempts.Count(att => !String.IsNullOrWhiteSpace(att.AttemptId))}");
            Logger.Info($"Detected containers:  {Model.Instance.AppAttempts.Sum(att => att.DetectedContainerCount)}");
            Logger.Info($"Checked Constraints:  {Oracle.SuTConstraintCheckedCount} SuT / {Oracle.TestConstraintCheckedCount} Test");
            Logger.Info($"Failed Constraints:   {Oracle.SuTConstraintFailedCount} SuT / {Oracle.TestConstraintFailedCount} Test");
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
        /// Prints the current controller trace to logging outputs
        /// </summary>
        /// <param name="controller">Client to print</param>
        public static void PrintTrace(YarnController controller)
        {
            if(YarnController.MarpValues != null)
            {
                var realStep = StepCount + 1;
                var secondVal = realStep * 2;
                var firstVal = secondVal - 1;
                Logger.Info("=== Controller ===");
                Logger.Info($"MARP Value on start: {YarnController.MarpValues[firstVal - 1]}");
                Logger.Info($"MARP value on end:   {YarnController.MarpValues[secondVal - 1]}");
            }
        }

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
            Logger.Info($"    CPU used/free: {node.CpuUsed}/{node.CpuAvailable} ({node.CpuUsage:F3})");
            Logger.Info($"    Health Report: {node.HealthReport}");
        }

        /// <summary>
        /// Prints the current client trace to logging outputs
        /// </summary>
        /// <param name="client">Client to print</param>
        public static void PrintTrace(Client client)
        {
            Logger.Info($"=== Client {client.ClientId} ===");
            Logger.Info($"    Current executing bench:  {client.BenchController?.CurrentBenchmark?.Name}");
            Logger.Info($"    Current executing app id: {client.CurrentExecutingAppId}");
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
            Logger.Info($"        IsKillable:  {app.IsKillable}");
            Logger.Info($"        AM Host:     {app.AmHostId} ({app.AmHost?.State})");
            Logger.Info($"        Diagnostics: {app.Diagnostics}");
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
            Logger.Info($"            Cont. Count:  {attempt.RunningContainerCount}");
            Logger.Info($"            Detected Cnt: {attempt.DetectedContainerCount}");
            Logger.Info($"            Diagnostics:  {attempt.Diagnostics}");
        }

        /// <summary>
        /// Prints the current container trace to logging outputs
        /// </summary>
        /// <param name="container">Container to print</param>
        public static void PrintTrace(YarnAppContainer container)
        {
            //Logger.Info($"          === Container {container.ContainerId} ===");
            //Logger.Info($"              State: {container.State}");
            //Logger.Info($"              Host:  {container.HostId} ({container.Host?.State})");
            Logger.Info($"          === Container {container.ContainerId}@{container.HostId}: {container.State}");
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
            PrintTrace(controller);

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

        #region Other output

        /// <summary>
        /// Prints on Debug level which constraint for which requirement number on which component is currently testing
        /// </summary>
        /// <param name="requirement">The requirement to check</param>
        /// <param name="component">The component</param>
        public static void PrintTestConstraint(string requirement, string component)
        {
            if(!String.IsNullOrWhiteSpace(component))
                component = $" ({component})";
            Logger.Debug($"Test constraint for {requirement} on {component}");
        }

        #endregion
    }
}