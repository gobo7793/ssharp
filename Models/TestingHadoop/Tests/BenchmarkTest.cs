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
        private BenchmarkController _Bench1;
        //private BenchmarkController _Bench2;
        //private BenchmarkController _Bench3;

        [SetUp]
        public void Setup()
        {
            _Bench1 = new BenchmarkController("", 1);
            //_Bench2 = new BenchmarkController("", 7);
            //_Bench3 = new BenchmarkController("", 11);

            /* Benchmarks for Seed 1:
                Bench 01:randomtextwriter
                Bench 02:randomtextwriter
                Bench 03:randomtextwriter
                Bench 04:sort            
                Bench 05:sort            
                Bench 06:sort            
                Bench 07:randomtextwriter
                Bench 08:dfsioePreparing 
                Bench 09:dfsioePreparing 
                Bench 10:dfsioePreparing 
                Bench 11:dfsioe          
                Bench 12:pi              
                Bench 13:pentomino       
                Bench 14:fail            
                Bench 15:fail            
                Bench 16:fail            
                Bench 17:fail            
                Bench 18:pentomino       
                Bench 19:pentomino       
                Bench 20:randomtextwriter
            */
        }

        [Test]
        public void TestTransitions()
        {
            _Bench1.InitStartBench();
            //_Bench2.InitStartBench();
            //_Bench3.InitStartBench();
            for(int i = 1; i < 100; i++)
            {
                Console.WriteLine($"Bench {i:D2}:{_Bench1.CurrentBenchmark.Name,-16}");
                _Bench1.ChangeBenchmark();
                //_Bench2.ChangeBenchmark();
                //_Bench3.ChangeBenchmark();
            }
        }
    }
}