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
using System.Linq;
using System.Reflection;
using ISSE.SafetyChecking.Modeling;
using NUnit.Framework;
using SafetySharp.CaseStudies.TestingHadoop.Modeling;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver.Parser;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    [TestFixture]
    public class AttributeTest
    {
        private Model _Model;
        private YarnNode _Node1;

        [TestFixtureSetUp]
        public void Setup()
        {
            _Model = Model.Instance;
            var parser = CmdParser.CreateInstance(new DummyHadoopCmdConnector());

            _Model.InitTestConfig(parser, parser.Connection);

            _Node1 = _Model.Nodes[0];
        }

        [Test]
        [TestCase(5, 5, 0.0)]
        [TestCase(0, 0, 0.5)]
        [TestCase(0, 0, 0.8)]
        [TestCase(5, 5, 1.0)]
        [TestCase(10, 10, 0.5)]
        [TestCase(5, 5, 0.5)]
        [TestCase(5, 5, 0.8)]
        [TestCase(5, 5, 0.2)]
        [TestCase(8, 8, 0.5)]
        [TestCase(2, 2, 0.5)]
        [TestCase(2, 2, 0.2)]
        [TestCase(8, 8, 0.8)]
        public void TestFaultActivation(int memUsage, int cpuUsage, double prob)
        {
            _Node1.MemoryAvailable = 10 - memUsage;
            _Node1.MemoryUsed = memUsage;
            _Node1.CpuAvailable = 10 - cpuUsage;
            _Node1.CpuUsed = cpuUsage;
            ModelSettings.FaultActivationProbability = prob;
            ModelSettings.FaultRepairProbability = prob;

            var faultTuple =
                (from faultField in _Node1.GetType().GetFields()
                 where typeof(Fault).IsAssignableFrom(faultField.FieldType)
                 let attribute = faultField.GetCustomAttribute<NodeFaultAttribute>()
                 where attribute != null
                 let fault = (Fault)faultField.GetValue(_Node1)
                 select Tuple.Create(fault, attribute)).First();

            Console.WriteLine($"Fault: {faultTuple.Item1.Name}");
            int a = 0, r = 0;
            var steps = 100;
            for(int i = 0; i < steps; i++)
            {
                var act = faultTuple.Item2.CanActivate(_Node1);
                var rep = faultTuple.Item2.CanRepair(_Node1);
                //Console.WriteLine($"Activation in step {i:D2}: {act}");
                //Console.WriteLine($"Repair     in step {i:D2}: {rep}");
                if(act) a++;
                if(rep) r++;
            }

            Console.WriteLine($"Activations: {a}/{steps}");
            Console.WriteLine($"Reparis:     {r}/{steps}");
        }
    }
}