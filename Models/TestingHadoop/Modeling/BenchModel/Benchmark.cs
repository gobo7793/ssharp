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

using System.Diagnostics;
using System.Text;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.BenchModel
{
    /// <summary>
    /// General data for executing a benchmark
    /// </summary>
    [DebuggerDisplay("Benchmark {" + nameof(Name) + "}")]
    public class Benchmark
    {
        private readonly StringBuilder _StartCmdBuilder;

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
        /// Gets the full start command for this benchmark (without the use of the benchmark start script)
        /// </summary>
        /// <param name="clientDir">The base directory to use on hdfs, empty for root directory</param>
        /// <returns>The start command without benchmark script</returns>
        public string GetStartCmd(string clientDir = "")
        {
            return _StartCmdBuilder.Replace("$DIR", clientDir).ToString();
        }

        /// <summary>
        /// Initializes a new Benchmark
        /// </summary>
        /// <param name="id">Internal benchmark id</param>
        /// <param name="name">Benchmark name</param>
        /// <param name="startCmd">Benchmark start command (without benchmark start script), set the hdfs base directory using $DIR</param>
        public Benchmark(int id, string name, string startCmd)
        {
            Id = id;
            Name = name;
            _StartCmdBuilder = new StringBuilder(startCmd);
        }
    }
}