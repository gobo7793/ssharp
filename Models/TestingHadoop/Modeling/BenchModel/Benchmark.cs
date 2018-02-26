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

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel
{
    /// <summary>
    /// Representing a benchmark application
    /// </summary>
    public class Benchmark
    {
        #region Properties

        private static readonly BenchmarkAppData _FailBench;
        private static readonly BenchmarkAppData _SleepBench;

        private BenchmarkAppData _PreviousBenchmark;
        private BenchmarkAppData _CurrentBenchmark;

        /// <summary>
        /// The available benchmark list
        /// </summary>
        public static BenchmarkAppData[] Benchmarks { get; }

        /// <summary>
        /// Transition probabilities from one benchmark (first dimension) to another (second dimension)
        /// </summary>
        public static int[][] BenchTransitions { get; }

        /// <summary>
        /// Base directory for the client
        /// </summary>
        public string ClientDir { get; set; }

        /// <summary>
        /// Random number generator
        /// </summary>
        private Random RandomGen { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the base benchmark settings
        /// </summary>
        static Benchmark()
        {
            //_FailBench = new BenchmarkAppData(12, "fail", "jobclient fail -failMappers 3");
            //_SleepBench = new BenchmarkAppData(13, "sleep", "jobclient sleep -m 3 -r 1 -mt 20 -mr 10");

            Benchmarks = new[]
            {
                // Benchmark IDs have to be in ascending ordner, they will be needed for transitions
                new BenchmarkAppData(00, "dfsioePreparing", "hibench bin/workloads/micro/dfsioe/prepare/prepare.sh"),
                new BenchmarkAppData(01, "randomtextwriter", "example randomtextwriter $DIR/rantw"),
                new BenchmarkAppData(02, "teragen", "example teragen 50000 $DIR/teragen"),
                new BenchmarkAppData(03, "dfsioe", "hibench bin/workloads/micro/dfsioe/hadoop/run.sh"),
                new BenchmarkAppData(04, "wordcount", "example wordcount $DIR/rantw $DIR/wcout"),
                new BenchmarkAppData(05, "randomwriter", "example randomwriter $DIR/ranwr"),
                new BenchmarkAppData(06, "sort", "example sort $DIR/rantw $DIR/sort"),
                new BenchmarkAppData(07, "terasort", "example terasort $DIR/teragen $DIR/terasort"),
                new BenchmarkAppData(08, "pi", "pi"),
                new BenchmarkAppData(09, "pentomino", "example pentomino $DIR/pent -depth 2 -heigh 10 -width 6"),
                new BenchmarkAppData(10, "testmapredsort", "jobclient testmapredsort -sortInput $DIR/rantw -sortOutput $DIR/sort"),
                new BenchmarkAppData(11, "teravalidate", "example teravalidate $DIR/terasort $DIR/teravalidate"),
                _FailBench = new BenchmarkAppData(12, "fail", "jobclient fail -failMappers 3"),
                _SleepBench = new BenchmarkAppData(13, "sleep", "jobclient sleep -m 3 -r 1 -mt 20 -mr 10"),
            };

            BenchTransitions = new[]
            {
                /* from benchmark id (down) to benchmark id (right)                      */
                /*               00  01  02  03  04  05  06  07  08  09  10  11  12  13  */
                new[] /* 00 */ { 50, 20, 00, 70, 00, 00, 00, 00, 10, 00, 00, 00, 05, 05, },
                new[] /* 01 */ { 10, 50, 00, 00, 40, 10, 30, 00, 10, 00, 00, 00, 05, 05, },
                new[] /* 02 */ { 00, 10, 50, 00, 00, 00, 00, 70, 00, 20, 00, 00, 05, 05, },
                new[] /* 03 */ { 00, 20, 00, 50, 00, 10, 00, 00, 40, 30, 00, 00, 05, 05, },
                new[] /* 04 */ { 20, 30, 00, 00, 50, 00, 20, 00, 20, 10, 00, 00, 05, 05, },
                new[] /* 05 */ { 00, 20, 20, 00, 00, 50, 00, 00, 30, 30, 00, 00, 05, 05, },
                new[] /* 06 */ { 00, 20, 10, 00, 20, 10, 50, 00, 20, 00, 20, 00, 05, 05, },
                new[] /* 07 */ { 00, 00, 00, 00, 00, 00, 00, 50, 30, 20, 00, 50, 05, 05, },
                new[] /* 08 */ { 40, 30, 00, 00, 00, 00, 00, 00, 50, 30, 00, 00, 05, 05, },
                new[] /* 09 */ { 30, 30, 00, 00, 00, 20, 00, 00, 20, 50, 00, 00, 05, 05, },
                new[] /* 10 */ { 00, 40, 00, 00, 00, 20, 00, 00, 10, 30, 50, 00, 05, 05, },
                new[] /* 11 */ { 20, 30, 00, 00, 00, 00, 00, 00, 30, 20, 00, 50, 05, 05, },
                //new[] /* 12 */ { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 50, 00, },
                //new[] /* 13 */ { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 50, },
                /* From fail and sleep always 50/50 back to last benchmark               */
                /* Without self-references and always possible fail+sleep all values     */
                /* should be cumulated 100, with it (50+5+5) 160                         */
            };
        }

        /// <summary>
        /// Initializes the benchmarks
        /// </summary>
        /// <param name="clientDir">The hdfs client directory</param>
        /// <param name="randomSeed">Seed for random generator (for debugging), on -1 a time based seed will be used</param>
        public Benchmark(string clientDir, int randomSeed = -1)
        {
            ClientDir = clientDir;
            if(randomSeed == -1)
                RandomGen = new Random();
            else
                RandomGen = new Random(randomSeed);
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Returns one of the start benchmarks
        /// </summary>
        /// <returns>The start benchmark</returns>
        /// <remarks>
        /// All possible start benchmarks will be selected with same probability.
        /// Possible start benchmarks are all that need no preparing or input data:
        /// dfsioe Preparing, randomtextgenerator, teragen, randomwriter, pi, pentomino
        /// </remarks>
        public BenchmarkAppData GetStartBench()
        {
            var startNo = RandomGen.Next(0, 6);
            switch(startNo)
            {
                case 0:
                    _CurrentBenchmark = Benchmarks[0]; // dfsioe preparing
                    break;
                case 1:
                    _CurrentBenchmark = Benchmarks[1]; // randomtextgenerator
                    break;
                case 2:
                    _CurrentBenchmark = Benchmarks[2]; // teragen
                    break;
                case 3:
                    _CurrentBenchmark = Benchmarks[5]; // randomwriter
                    break;
                case 4:
                    _CurrentBenchmark = Benchmarks[8]; // pi
                    break;
                case 5:
                    _CurrentBenchmark = Benchmarks[9]; // pentomino
                    break;
                default: throw new Exception("invalid random number on selecting start benchmark");
            }

            return _CurrentBenchmark;
        }

        /// <summary>
        /// Changes the current executed benchmark based on <see cref="BenchTransitions"/> definied probabilities
        /// </summary>
        /// <returns>The next benchmark to execute</returns>
        /// <exception cref="InvalidOperationException">If <see cref="BenchTransitions"/> is not in needed format</exception>
        /// <exception cref="Exception">If no followin benchmark found</exception>
        public BenchmarkAppData ChangeBenchmark()
        {
            if(BenchTransitions.Length != Benchmarks.Length - 2)
                throw new InvalidOperationException("Complete benchmark transition array must have same length like benchmark array without fail and sleep on end");

            if(_CurrentBenchmark == _FailBench || _CurrentBenchmark == _SleepBench)
            {
                var backProbability = RandomGen.NextDouble();
                if(backProbability < 0.5)
                {
                    var curr = _CurrentBenchmark;
                    _CurrentBenchmark = _PreviousBenchmark;
                    _PreviousBenchmark = curr;
                }
            }
            else
            {
                var transitions = BenchTransitions[_CurrentBenchmark.Id];
                if(transitions.Length != Benchmarks.Length)
                    throw new InvalidOperationException(
                        $"Benchmark transition array for benchmark {_CurrentBenchmark.Name} must have same length like benchmark array");

                var ranNumber = RandomGen.Next(160);
                var cumulative = 0;
                for(int i = 0; i < transitions.Length; i++)
                {
                    cumulative += transitions[i];
                    if(ranNumber >= cumulative)
                        continue;

                    if(_CurrentBenchmark != Benchmarks[i])
                    {
                        _PreviousBenchmark = _CurrentBenchmark;
                        _CurrentBenchmark = Benchmarks[i];
                    }
                    break;
                }
            }

            return _CurrentBenchmark;
        }

        #endregion
    }
}