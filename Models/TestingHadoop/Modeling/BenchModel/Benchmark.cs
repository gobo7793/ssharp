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
using System.Diagnostics;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel
{
    /// <summary>
    /// General data for executing a benchmark
    /// </summary>
    [DebuggerDisplay("Benchmark {" + nameof(Name) + "}")]
    public class Benchmark
    {
        #region Properties

        /// <summary>
        /// Base directory placeholder
        /// </summary>
        public const string BaseDirHolder = "$DIR";

        /// <summary>
        /// Output directory placeholder
        /// </summary>
        public const string OutDirHolder = "$OUT";

        /// <summary>
        /// Input directory placeholder
        /// </summary>
        public const string InDirHolder = "$IN";

        private readonly string _StartCmd;
        private readonly string _OutDir;
        private readonly string _InDir;

        /// <summary>
        /// Internal benchmark id
        /// </summary>
        /// <remarks>
        /// Must be for all benchmarks in ascending order
        /// </remarks>
        public int Id { get; }

        /// <summary>
        /// Benchmark name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Indicates if the benchmark has a hdfs output directory
        /// </summary>
        public bool HasOutputDir { get; }

        /// <summary>
        /// Indicates if the benchmark has a hdfs input directory
        /// </summary>
        public bool HasInputDir { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new Benchmark
        /// </summary>
        /// <param name="id">Internal benchmark id</param>
        /// <param name="name">Benchmark name</param>
        /// <param name="startCmd">Benchmark start command (without benchmark start script)</param>
        public Benchmark(int id, string name, string startCmd)
        : this(id, name, startCmd, String.Empty, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new Benchmark
        /// </summary>
        /// <param name="id">Internal benchmark id</param>
        /// <param name="name">Benchmark name</param>
        /// <param name="startCmd">Benchmark start command (without benchmark start script), set outputDir via <see cref="OutDirHolder"/></param>
        /// <param name="outputDir">Output directory, set the hdfs base directory using <see cref="BaseDirHolder"/></param>
        public Benchmark(int id, string name, string startCmd, string outputDir)
            : this(id, name, startCmd, outputDir, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new Benchmark
        /// </summary>
        /// <param name="id">Internal benchmark id</param>
        /// <param name="name">Benchmark name</param>
        /// <param name="startCmd">Benchmark start command (without benchmark start script),
        ///     set outputDir via <see cref="OutDirHolder"/>, inputDirectory via <see cref="InDirHolder"/></param>
        /// <param name="outputDir">Output directory, set the hdfs base directory using <see cref="BaseDirHolder"/></param>
        /// <param name="inputDir">Input directory, set the hdfs base directory using <see cref="BaseDirHolder"/></param>
        public Benchmark(int id, string name, string startCmd, string outputDir, string inputDir)
        {
            Id = id;
            Name = name;
            _StartCmd = startCmd;
            HasOutputDir = !String.IsNullOrWhiteSpace(outputDir);
            _OutDir = outputDir;
            HasInputDir = !String.IsNullOrWhiteSpace(inputDir);
            _InDir = inputDir;
        }

        #endregion

        #region Methods

        private string ReplaceClientDir(string dir, string clientDir)
        {
            if(String.IsNullOrWhiteSpace(dir))
                return String.Empty;
            return dir.Replace(BaseDirHolder, clientDir);
        }

        /// <summary>
        /// Gets the full start command for this benchmark (without the use of the benchmark start script)
        /// </summary>
        /// <param name="clientDir">The base directory to use on hdfs, empty for root directory</param>
        /// <returns>The start command without benchmark script</returns>
        public string GetStartCmd(string clientDir = "")
        {
            var result = _StartCmd.Replace(OutDirHolder, GetOutputDir(clientDir)).Replace(InDirHolder, GetInputDir(clientDir));
            if(result.Contains(BaseDirHolder))
                result = ReplaceClientDir(result, clientDir);
            return result;
        }

        /// <summary>
        /// Gets the hdfs output directory for this benchmark
        /// </summary>
        /// <param name="clientDir">The base directory to use on hdfs, empty for root directory</param>
        /// <returns>The output directory</returns>
        public string GetOutputDir(string clientDir = "")
        {
            return ReplaceClientDir(_OutDir, clientDir);
        }

        /// <summary>
        /// Gets the hdfs input directory for this benchmark
        /// </summary>
        /// <param name="clientDir">The base directory to use on hdfs, empty for root directory</param>
        /// <returns>The input directory</returns>
        public string GetInputDir(string clientDir = "")
        {
            if(ModelSettings.IsPrecreateBenchInputs)
                clientDir = ModelSettings.PrecreateBenchInputsBaseDir;
            return ReplaceClientDir(_InDir, clientDir);
        }

        #endregion
    }
}