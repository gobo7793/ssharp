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
using static SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel.Benchmark;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel
{
    /// <summary>
    /// Controller for benchmarks
    /// </summary>
    public class BenchmarkController
    {
        #region Properties

        private static readonly Benchmark _SleepBench;
        private Benchmark[] _BenchmarksInstance; // dummy instance to use to prevent S# errors

        /// <summary>
        /// The available benchmark list
        /// </summary>
        public static Benchmark[] Benchmarks { get; }

        /// <summary>
        /// Transition probabilities from one benchmark (first dimension) to another (second dimension)
        /// </summary>
        public static double[][] BenchTransitions { get; }

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
                new Benchmark(00, "dfsiowrite", $"jobclient TestDFSIO -Dtest.build.data={OutDirHolder} -read -nrFiles 12 -size 100MB",
                    $"{BaseDirHolder}/dfsio"),
                new Benchmark(01, "randomtextwriter",
                    $"example randomtextwriter -D mapreduce.randomtextwriter.totalbytes=48000 {OutDirHolder}", $"{BaseDirHolder}/rantw"),
                new Benchmark(02, "teragen", $"example teragen 48000 {OutDirHolder}", $"{BaseDirHolder}/teragen"),
                new Benchmark(03, "dfsioread", $"jobclient TestDFSIO -Dtest.build.data={OutDirHolder} -read -nrFiles 12 -size 100MB",
                    $"{BaseDirHolder}/dfsio"),
                new Benchmark(04, "wordcount", $"example wordcount {InDirHolder} {OutDirHolder}", $"{BaseDirHolder}/wcout",
                    $"{BaseDirHolder}/rantw"),
                new Benchmark(05, "randomwriter", $"example randomwriter -D mapreduce.randomwriter.totalbytes=48000 {OutDirHolder}",
                    $"{BaseDirHolder}/ranwr"),
                new Benchmark(06, "sort",
                    $"example sort -outKey org.apache.hadoop.io.Text -outValue org.apache.hadoop.io.Text {InDirHolder} {OutDirHolder}",
                    $"{BaseDirHolder}/sort", $"{BaseDirHolder}/rantw"),
                new Benchmark(07, "terasort", $"example terasort {InDirHolder} {OutDirHolder}", $"{BaseDirHolder}/terasort",
                    $"{BaseDirHolder}/teragen"),
                new Benchmark(08, "pi", "example pi 3 100"),
                new Benchmark(09, "pentomino", $"example pentomino {OutDirHolder} -depth 2 -heigh 10 -width 6", $"{BaseDirHolder}/pent"),
                new Benchmark(10, "testmapredsort", $"jobclient testmapredsort -sortInput {InDirHolder} -sortOutput {OutDirHolder}",
                    $"{BaseDirHolder}/sort", $"{BaseDirHolder}/rantw"),
                new Benchmark(11, "teravalidate", $"example teravalidate {InDirHolder} {OutDirHolder}", $"{BaseDirHolder}/teravalidate",
                    $"{BaseDirHolder}/terasort"),
                _SleepBench = new Benchmark(12, "sleep", "jobclient sleep -m 1 -r 1 -mt 10 -mr 5"),
                new Benchmark(13, "fail", "jobclient fail -failMappers 3"),
            };

            // change probabilities for benchmark transition system here!
            BenchTransitions = new[]
            {
                /* from / to ->    00    01   02    03    04    05    06    07    08    09    10    11    12    13  */
                /* from / to ->   dfw   rtw   tg   dfr    wc    rw    so   tsr    pi    pt   tms   tvl    sl    fl  */
                new[] /* 00 */ { .600, .073,  000, .145,  000,  000,  000,  000, .073, .073,  000,  000, .018, .018 },
                new[] /* 01 */ { .036, .600,  000,  000, .145, .036, .109,  000, .036,  000,  000,  000, .019, .019 },
                new[] /* 02 */ {  000, .036, .600,  000,  000,  000,  000, .255,  000, .073,  000,  000, .018, .018 },
                new[] /* 03 */ {  000, .073,  000, .600,  000, .036,  000,  000, .145, .109,  000,  000, .018, .019 },
                new[] /* 04 */ { .073, .109,  000,  000, .600,  000, .073,  000, .073, .036,  000,  000, .018, .018 },
                new[] /* 05 */ {  000, .073, .073,  000,  000, .600,  000,  000, .109, .109,  000,  000, .018, .018 },
                new[] /* 06 */ {  000, .073, .036,  000, .073, .036, .600,  000, .073,  000, .073,  000, .018, .018 },
                new[] /* 07 */ {  000,  000,  000,  000,  000,  000,  000, .600, .109, .073,  000, .182, .018, .018 },
                new[] /* 08 */ { .145, .109,  000,  000,  000,  000,  000,  000, .600, .109,  000,  000, .018, .019 },
                new[] /* 09 */ { .109, .109,  000,  000,  000, .073,  000,  000, .073, .600,  000,  000, .018, .018 },
                new[] /* 10 */ {  000, .145,  000,  000,  000, .073,  000,  000, .036, .109, .600,  000, .018, .019 },
                new[] /* 11 */ { .073, .109,  000,  000,  000,  000,  000,  000, .109, .073,  000, .600, .018, .018 },
                new[] /* 12 */ { 1/6D, 1/6D, 1/6D,  000,  000, 1/6D,  000,  000, 1/6D, 1/6D,  000,  000,  000,  000 },
                new[] /* 13 */ { 1/6D, 1/6D, 1/6D,  000,  000, 1/6D,  000,  000, 1/6D, 1/6D,  000,  000,  000,  000 },
            };
        }

        /// <summary>
        /// Initializes the benchmarks
        /// </summary>
        /// <param name="randomSeed">Seed for random generator</param>
        public BenchmarkController(int randomSeed)
        {
            RandomGen = new Random(randomSeed);
            InitStartBench();
        }

        /// <summary>
        /// Initializes the benchmarks
        /// </summary>
        public BenchmarkController()
        {
            RandomGen = new Random();
            InitStartBench();
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Initializes the start benchmark
        /// </summary>
        /// <remarks>
        /// To select the initial benchmark the transitions based on
        /// sleep benchmark saved in <see cref="_SleepBench"/> will be used.
        /// </remarks>
        public void InitStartBench()
        {
            _BenchmarksInstance = Benchmarks;
            PreviousBenchmark = _SleepBench;
            CurrentBenchmark = _SleepBench;
            //ChangeBenchmark();
        }

        /// <summary>
        /// Changes the current executed benchmark based on <see cref="BenchTransitions"/> definied probabilities
        /// and returns true on benchmark change
        /// </summary>
        /// <returns>True if the benchmark was changed</returns>
        /// <exception cref="InvalidOperationException">If <see cref="BenchTransitions"/> length is not same with <see cref="Benchmarks"/></exception>
        /// <exception cref="Exception">If no followin benchmark found</exception>
        public bool ChangeBenchmark()
        {
            if(BenchTransitions.Length != _BenchmarksInstance.Length)
                throw new InvalidOperationException("Complete benchmark transition array must have same length like benchmark array");

            // use transition system
            var transitions = BenchTransitions[CurrentBenchmark.Id];
            if(transitions.Length != _BenchmarksInstance.Length)
                throw new InvalidOperationException(
                    $"BenchmarkController transition array for benchmark {CurrentBenchmark.Name} must have same length like benchmark array");

            var ranNumber = RandomGen.NextDouble();
            var cumulative = 0D;
            for(int i = 0; i < transitions.Length; i++)
            {
                cumulative += transitions[i];
                if(ranNumber >= cumulative)
                    continue;

                // prevent saving current benchmark as previous
                if(CurrentBenchmark == _BenchmarksInstance[i])
                    break;

                PreviousBenchmark = CurrentBenchmark;
                CurrentBenchmark = _BenchmarksInstance[i];
                return true;
            }

            return false;
        }

        #endregion
    }
}