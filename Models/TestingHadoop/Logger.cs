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
using System.Runtime.CompilerServices;

namespace SafetySharp.CaseStudies.TestingHadoop
{
    /// <summary>
    /// Logging helper
    /// </summary>
    public static class Logger
    {
        #region Log level enum

        /// <summary>
        /// Possible logging levels
        /// </summary>
        public enum Level
        {
            Info = 0,
            Log = 1,
            Warning = 2,
            Exception = 3,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Logging directory, default &lt;<see cref="Environment.CurrentDirectory"/>&gt;/logs/
        /// </summary>
        public static string TargetDirectory => $"{Environment.CurrentDirectory}/logs";

        /// <summary>
        /// Logging target file name, default &lt;<see cref="DateTime.Today"/>&gt;.log
        /// </summary>
        public static string TargetFileName => $"{DateTime.Today:yyyy-MM-dd}.log";

        /// <summary>
        /// The full logging file path, based on <see cref="TargetDirectory"/>/<see cref="TargetFileName"/>
        /// </summary>
        public static string TargetFullPath => $"{TargetDirectory}/{TargetFileName}";

        /// <summary>
        /// The logging level
        /// </summary>
        public static Level LogLevel = Level.Log;

        #endregion

        #region Methods

        /// <summary>
        /// Logs the given line with timestamp, class name and line number into <see cref="TargetFileName"/> on level <see cref="Level.Info"/>
        /// </summary>
        /// <param name="line">The line to write</param>
        /// <param name="filePath">The calling class file path, default filled by compiler</param>
        /// <param name="lineNumber">The calling line number, default filled by compiler</param>
        public static void Info(string line = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            WriteLog(Level.Info, line, Path.GetFileName(filePath), lineNumber);
        }

        /// <summary>
        /// Logs the given line with timestamp, class name and line number into <see cref="TargetFileName"/> on level <see cref="Level.Log"/>
        /// </summary>
        /// <param name="line">The line to write</param>
        /// <param name="filePath">The calling class file path, default filled by compiler</param>
        /// <param name="lineNumber">The calling line number, default filled by compiler</param>
        public static void Log(string line = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            WriteLog(Level.Log, line, Path.GetFileName(filePath), lineNumber);
        }

        /// <summary>
        /// Logs the given line with timestamp, class name and line number into <see cref="TargetFileName"/> on level <see cref="Level.Warning"/>
        /// </summary>
        /// <param name="line">The line to write</param>
        /// <param name="filePath">The calling class file path, default filled by compiler</param>
        /// <param name="lineNumber">The calling line number, default filled by compiler</param>
        public static void Warning(string line = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            WriteLog(Level.Warning, line, Path.GetFileName(filePath), lineNumber);
        }

        /// <summary>
        /// Logs the given line with timestamp, class name and line number into <see cref="TargetFileName"/> on level <see cref="Level.Exception"/>
        /// </summary>
        /// <param name="line">The line to write</param>
        /// <param name="filePath">The calling class file path, default filled by compiler</param>
        /// <param name="lineNumber">The calling line number, default filled by compiler</param>
        public static void Exception(string line = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            WriteLog(Level.Exception, line, Path.GetFileName(filePath), lineNumber);
        }

        /// <summary>
        /// Logs the given line
        /// </summary>
        /// <param name="neededLevel">Needed <see cref="LogLevel"/> to log</param>
        /// <param name="line">The line to log</param>
        /// <param name="className">The calling class name</param>
        /// <param name="lineNumber">The calling line number</param>
        private static void WriteLog(Level neededLevel, string line, string className, int lineNumber)
        {
            if(LogLevel > neededLevel)
                return;

            var logged = String.IsNullOrWhiteSpace(line)
                ? $"{String.Empty}{Environment.NewLine}"
                : $"[{DateTime.Now:T}|{className}|L{lineNumber:##000}] {line}{Environment.NewLine}";

            Console.WriteLine(logged);

            if(!Directory.Exists(TargetDirectory))
                Directory.CreateDirectory(TargetDirectory);
            File.AppendAllText(TargetFullPath, logged);
        }

        #endregion
    }
}