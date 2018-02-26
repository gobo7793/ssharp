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
using NUnit.Framework;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Tests
{
    [TestFixture]
    public class BenchmarkTest
    {
        private Benchmark _Bench;

        [SetUp]
        public void Setup()
        {
            _Bench = new Benchmark("", 3);
            // Seed 3: 
            // Bench 00: randomtextwriter
            // Bench 01: sort
            // Bench 02: testmapredsort
            // Bench 03: randomtextwriter
            // Bench 04: wordcount
            // Bench 05: randomtextwriter
            // Bench 06: randomtextwriter
            // Bench 07: fail
            // Bench 08: randomtextwriter
            // Bench 09: wordcount
        }

        [Test]
        public void TestTransitions()
        {
            var bench = _Bench.GetStartBench();
            Console.WriteLine($"Bench 00: {bench.Name}");

            for(int i = 1; i < 100; i++)
            {
                bench = _Bench.ChangeBenchmark();
                Console.WriteLine($"Bench {i:D2}: {bench.Name,-20},cmd={bench.GetStartCmd()}");
            }
        }
    }
}