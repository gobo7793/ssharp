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

using System;
using System.Reflection;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// Attribute to set parameters to active the faults on <see cref="YarnNode"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NodeFaultAttribute : Attribute
    {
        #region Properties

        private static log4net.ILog Logger { get; } =
            log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Random number generator
        /// </summary>
        public static Random RandomGen { get; internal set; } = new Random();

        /// <summary>
        /// Fault activation probability, based on <see cref="ModelSettings.FaultActivationProbability"/>
        /// </summary>
        [NonSerializable]
        public double ActivationProbability => ModelSettings.FaultActivationProbability;

        /// <summary>
        /// Fault repair probability, based on <see cref="ModelSettings.FaultRepairProbability"/>
        /// </summary>
        [NonSerializable]
        public double RepairProbability => ModelSettings.FaultRepairProbability;

        /// <summary>
        /// Node usage of the node on fault activation time
        /// </summary>
        private double NodeUsageOnActivation { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Calculates the current node usage based on memory and cpu usage (between 0 and 2).
        /// Saves also the returning usage in <see cref="NodeUsageOnActivation"/>.
        /// </summary>
        /// <param name="node">The node</param>
        /// <returns>The usage saved in <see cref="NodeUsageOnActivation"/></returns>
        private double GetCurrentNodeUsage(YarnNode node)
        {
            var nodeUsage = (node.MemoryUsage + node.CpuUsage) / 2;

            if(nodeUsage < 0.1) nodeUsage = 0.1;
            else if(nodeUsage > 0.9) nodeUsage = 0.9;

            NodeUsageOnActivation = nodeUsage;
            return nodeUsage;
        }

        /// <summary>
        /// Gets the current probability to toggle the fault state. Based on current/last saved Memory and CPU usage of the node.
        /// </summary>
        /// <returns>The probability to toggle the fault</returns>
        private double GetFaultProbability(YarnNode node)
        {
            var nodeUsage = GetCurrentNodeUsage(node);
            var faultUsage = nodeUsage * ActivationProbability * 2;

            return 1 - faultUsage;
        }

        /// <summary>
        /// Indicates if the fault can be activated. The probability will be calculated
        /// based on the node usage, <see cref="ActivationProbability"/> and a random generated number.
        /// </summary>
        /// <param name="node">The node of the fault to calculate the probability</param>
        /// <returns>True if the fault can be activated</returns>
        public bool CanActivate(YarnNode node)
        {
            var probability = GetFaultProbability(node);
            //Console.WriteLine(probability);
            var randomValue = RandomGen.NextDouble();
            Logger.Info($"Activation probability: {probability} < {randomValue}");
            return probability < randomValue;
        }

        /// <summary>
        /// Gets the current probability to toggle the fault state. Based on current/last saved Memory and CPU usage of the node.
        /// </summary>
        /// <returns>The probability to toggle the fault</returns>
        private double GetRepairProbability()
        {
            var nodeUsage = NodeUsageOnActivation;
            var faultUsage = nodeUsage * RepairProbability * 2;
            return 1 - faultUsage;
        }

        /// <summary>
        /// Indicates if the fault can be repaired. The probability will be calculated
        /// based on the node usage, <see cref="RepairProbability"/> and a random generated number.
        /// </summary>
        /// <returns>True if the fault can be repaired</returns>
        public bool CanRepair()
        {
            var probability = GetRepairProbability();
            //Console.WriteLine(probability);
            var randomValue = RandomGen.NextDouble();
            Logger.Info($"Repairing probability: {probability} > {randomValue}");
            return probability > randomValue;
        }

        #endregion
    }
}