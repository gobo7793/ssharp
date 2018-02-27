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
    /// Controller for benchmarks
    /// </summary>
    public class BenchmarkController
    {
        #region Properties

        private static readonly Benchmark _FailBench;
        private static readonly Benchmark _SleepBench;
        private static readonly int _TransitionCumulatedProbability;
        private static readonly int _SelfTransitionProbability;

        /// <summary>
        /// The available benchmark list
        /// </summary>
        public static Benchmark[] Benchmarks { get; }

        /// <summary>
        /// Transition probabilities from one benchmark (first dimension) to another (second dimension)
        /// </summary>
        public static int[][] BenchTransitions { get; }

        /// <summary>
        /// Random number generator
        /// </summary>
        private Random RandomGen { get; }

        /// <summary>
        /// Indicates if the controller is initialized with starting benchmark
        /// </summary>
        public bool IsInit => CurrentBenchmark != null;

        /// <summary>
        /// Previous executed benchmark
        /// </summary>
        public Benchmark PreviousBenchmark { get; set; }

        /// <summary>
        /// Current executing benchmark
        /// </summary>
        public Benchmark CurrentBenchmark { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the base benchmark settings
        /// </summary>
        static BenchmarkController()
        {
            Benchmarks = new[]
            {
                // BenchmarkController IDs have to be in ascending ordner, they will be needed for transitions
                new Benchmark(00, "dfsioePreparing", "hibench bin/workloads/micro/dfsioe/prepare/prepare.sh"),
                new Benchmark(01, "randomtextwriter", "example randomtextwriter $DIR/rantw"),
                new Benchmark(02, "teragen", "example teragen 50000 $DIR/teragen"),
                new Benchmark(03, "dfsioe", "hibench bin/workloads/micro/dfsioe/hadoop/run.sh"),
                new Benchmark(04, "wordcount", "example wordcount $DIR/rantw $DIR/wcout"),
                new Benchmark(05, "randomwriter", "example randomwriter $DIR/ranwr"),
                new Benchmark(06, "sort", "example sort $DIR/rantw $DIR/sort"),
                new Benchmark(07, "terasort", "example terasort $DIR/teragen $DIR/terasort"),
                new Benchmark(08, "pi", "pi"),
                new Benchmark(09, "pentomino", "example pentomino $DIR/pent -depth 2 -heigh 10 -width 6"),
                new Benchmark(10, "testmapredsort", "jobclient testmapredsort -sortInput $DIR/rantw -sortOutput $DIR/sort"),
                new Benchmark(11, "teravalidate", "example teravalidate $DIR/terasort $DIR/teravalidate"),
                _FailBench = new Benchmark(12, "fail", "jobclient fail -failMappers 3"),
                _SleepBench = new Benchmark(13, "sleep", "jobclient sleep -m 1 -r 1 -mt 10 -mr 5"),
            };

            // change probabilities for benchmark transition system here!
            // note: from fail and sleep always 50/50 back to previous benchmark
            BenchTransitions = new[]
            {
                /* from / to ->  00  01  02  03  04  05  06  07  08  09  10  11  12  13  */
                new[] /* 00 */ {  0, 20, 00, 70, 00, 00, 00, 00, 10, 00, 00, 00, 05, 05, },
                new[] /* 01 */ { 10,  0, 00, 00, 40, 10, 30, 00, 10, 00, 00, 00, 05, 05, },
                new[] /* 02 */ { 00, 10,  0, 00, 00, 00, 00, 70, 00, 20, 00, 00, 05, 05, },
                new[] /* 03 */ { 00, 20, 00,  0, 00, 10, 00, 00, 40, 30, 00, 00, 05, 05, },
                new[] /* 04 */ { 20, 30, 00, 00,  0, 00, 20, 00, 20, 10, 00, 00, 05, 05, },
                new[] /* 05 */ { 00, 20, 20, 00, 00,  0, 00, 00, 30, 30, 00, 00, 05, 05, },
                new[] /* 06 */ { 00, 20, 10, 00, 20, 10,  0, 00, 20, 00, 20, 00, 05, 05, },
                new[] /* 07 */ { 00, 00, 00, 00, 00, 00, 00,  0, 30, 20, 00, 50, 05, 05, },
                new[] /* 08 */ { 40, 30, 00, 00, 00, 00, 00, 00,  0, 30, 00, 00, 05, 05, },
                new[] /* 09 */ { 30, 30, 00, 00, 00, 20, 00, 00, 20,  0, 00, 00, 05, 05, },
                new[] /* 10 */ { 00, 40, 00, 00, 00, 20, 00, 00, 10, 30,  0, 00, 05, 05, },
                new[] /* 11 */ { 20, 30, 00, 00, 00, 00, 00, 00, 30, 20, 00,  0, 05, 05, },
            };
            _TransitionCumulatedProbability = 110; // cumulated transition probabilities w/o self-references
            _SelfTransitionProbability = 60; // self-transiion probability in percent
        }

        /// <summary>
        /// Initializes the benchmarks
        /// </summary>
        /// <param name="randomSeed">Seed for random generator</param>
        public BenchmarkController(int randomSeed)
        {
            RandomGen = new Random(randomSeed);
        }

        /// <summary>
        /// Initializes the benchmarks
        /// </summary>
        public BenchmarkController()
        {
            RandomGen = new Random();
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Initializes the start benchmark
        /// </summary>
        /// <remarks>
        /// All possible start benchmarks will be selected with same probability.
        /// Possible start benchmarks are all that need no preparing or input data:
        /// dfsioe Preparing, randomtextgenerator, teragen, randomwriter, pi, pentomino
        /// </remarks>
        public void InitStartBench()
        {
            var startNo = RandomGen.Next(0, 6);
            switch(startNo)
            {
                case 0:
                    CurrentBenchmark = Benchmarks[0]; // dfsioe preparing
                    break;
                case 1:
                    CurrentBenchmark = Benchmarks[1]; // randomtextgenerator
                    break;
                case 2:
                    CurrentBenchmark = Benchmarks[2]; // teragen
                    break;
                case 3:
                    CurrentBenchmark = Benchmarks[5]; // randomwriter
                    break;
                case 4:
                    CurrentBenchmark = Benchmarks[8]; // pi
                    break;
                case 5:
                    CurrentBenchmark = Benchmarks[9]; // pentomino
                    break;
                default: throw new Exception("invalid random number on selecting start benchmark");
            }
        }

        /// <summary>
        /// Changes the current executed benchmark based on <see cref="BenchTransitions"/> definied probabilities
        /// and returns true on benchmark change
        /// </summary>
        /// <returns>True if the benchmark was changed</returns>
        /// <exception cref="InvalidOperationException">If <see cref="BenchTransitions"/> is not in needed format</exception>
        /// <exception cref="Exception">If no followin benchmark found</exception>
        public bool ChangeBenchmark()
        {
            if(BenchTransitions.Length != Benchmarks.Length - 2)
                throw new InvalidOperationException("Complete benchmark transition array must have same length like benchmark array without fail and sleep on end");

            var selfProbability = RandomGen.Next(100);

            // back from fail and sleep
            if(CurrentBenchmark == _FailBench || CurrentBenchmark == _SleepBench)
            {
                if(selfProbability < 50)
                    return false;

                var curr = CurrentBenchmark;
                CurrentBenchmark = PreviousBenchmark;
                PreviousBenchmark = curr;
                return true;
            }

            // self-transition
            if(selfProbability < _SelfTransitionProbability)
                return false;

            // use transition system
            var transitions = BenchTransitions[CurrentBenchmark.Id];
            if(transitions.Length != Benchmarks.Length)
                throw new InvalidOperationException(
                    $"BenchmarkController transition array for benchmark {CurrentBenchmark.Name} must have same length like benchmark array");

            var ranNumber = RandomGen.Next(_TransitionCumulatedProbability);
            var cumulative = 0;
            for(int i = 0; i < transitions.Length; i++)
            {
                cumulative += transitions[i];
                if(ranNumber >= cumulative)
                    continue;
                if(CurrentBenchmark == Benchmarks[i])
                    break;

                PreviousBenchmark = CurrentBenchmark;
                CurrentBenchmark = Benchmarks[i];
                return true;
            }

            return false;
        }

        #endregion
    }
}