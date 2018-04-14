#region License
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
#endregion

using System;
using System.IO;

namespace SafetySharp.CaseStudies.TestingHadoop.Analysis
{
    /// <summary>
    /// Logging class
    /// </summary>
    public static class Logger
    {
        #region Properties

        /// <summary>
        /// Logging directory, default &lt;workingDic&gt;/logs/
        /// </summary>
        public static string TargetDirectory => $"{Environment.CurrentDirectory}/logs";

        /// <summary>
        /// Logging target file name, default &lt;today&gt;.log
        /// </summary>
        public static string TargetFileName => $"{TargetDirectory}/{DateTime.Today:yyyy-MM-dd}.log";

        #endregion

        #region Methods

        /// <summary>
        /// Logs the given line with timestamp into <see cref="TargetFileName"/>
        /// </summary>
        /// <param name="line">The line to write</param>
        public static void Log(string line)
        {
            if(!Directory.Exists(TargetDirectory))
                Directory.CreateDirectory(TargetDirectory);
            File.AppendAllText(TargetFileName, line);
        }

        #endregion
    }
}