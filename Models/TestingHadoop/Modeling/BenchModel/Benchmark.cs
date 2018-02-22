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
using System.Collections.Generic;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel
{
    /// <summary>
    /// Representing a benchmark application
    /// </summary>
    public class Benchmark
    {
        #region Properties

        /// <summary>
        /// Base directory for the client
        /// </summary>
        public string ClientDir { get; set; }

        /// <summary>
        /// The available benchmark list
        /// </summary>
        public static List<BenchmarkAppData> Benchmarks { get; set; }

        #endregion

        public Benchmark(string clientDir)
        {
            ClientDir = clientDir;

            Benchmarks = new List<BenchmarkAppData>
            {
                new BenchmarkAppData{Id=0, Name="randomtextwriter", StartCmd="example randomtextwriter {0}", OutDir=$"{ClientDir}/testing/rantw"},
                new BenchmarkAppData{Id=1, Name="wordcount", StartCmd=$"example wordcount {ClientDir}/rantw {{0}}", OutDir=$"{ClientDir}/wcout"},
                new BenchmarkAppData{Id=1, Name="dfsioePre", StartCmd="hibench bin/workloads/micro/dfsioe/prepare/prepare.sh"},
                new BenchmarkAppData{Id=2, Name="dfsioe", StartCmd="hibench bin/workloads/micro/dfsioe/hadoop/run.sh"},
                new BenchmarkAppData{Id=3, Name="pi", StartCmd="pi"},
                new BenchmarkAppData{Id=4, Name="randomwriter", StartCmd="example randomwriter {0}", OutDir=$"{ClientDir}/ranwr"},
                new BenchmarkAppData{Id=5, Name="pentomino", StartCmd="example pentomino -depth 2 -heigh 10 -width 6", OutDir=$"{ClientDir}/pent"},
                new BenchmarkAppData{Id=6, Name="sort", StartCmd=$"example sort {ClientDir}/rantw {{0}}", OutDir=$"{ClientDir}/sort"},
                new BenchmarkAppData{Id=7, Name="teragen", StartCmd="example teragen 50000 {0}", OutDir=$"{ClientDir}/teragen"},
                new BenchmarkAppData{Id=8, Name="terasort", StartCmd=$"example terasort {ClientDir}/teragen {{0}}", OutDir=$"{ClientDir}/terasort"},
                new BenchmarkAppData{Id=9, Name="teravalidate", StartCmd=$"example teravalidate {ClientDir}/terasort {{0}}", OutDir=$"{ClientDir}/teravalidate"},
                new BenchmarkAppData{Id=10, Name="testmapredsort", StartCmd=$"jobclient testmapredsort -sortInput {ClientDir}/rantw -sortOutput {ClientDir}/sort"},
                new BenchmarkAppData{Id=11, Name="fail", StartCmd="jobclient fail -failMappers 3"},
                new BenchmarkAppData{Id=12, Name="sleep", StartCmd="jobclient sleep -m 3 -r 1 -mt 20 -mr 10"},
            };
        }

        /// <summary>
        /// Representing general benchmark data
        /// </summary>
        public struct BenchmarkAppData
        {
            private string _StartCmd;

            /// <summary>
            /// Internal benchmark id
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Benchmark name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Benchmark start command (without bench.sh). Insert <see cref="OutDir"/> with {0}.
            /// </summary>
            public string StartCmd
            {
                get { return String.Format(_StartCmd, OutDir); }
                set { _StartCmd = value; }
            }

            /// <summary>
            /// Output directory
            /// </summary>
            public string OutDir { get; set; }
        }
    }
}