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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver;
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// Execution Container for <see cref="AppAttempt"/>s
    /// </summary>
    [DebuggerDisplay("Container {" + nameof(ContainerId) + "}")]
    public class YarnAppContainer : Component, IYarnReadable
    {
        #region Properties

        /// <summary>
        /// Current State
        /// </summary>
        public EContainerState State { get; set; } = EContainerState.None;

        /// <summary>
        /// Container ID
        /// </summary>
        public string ContainerId { get; set; }

        /// <summary>
        /// Starting Time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Ending Time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// <see cref="YarnNode"/> running this container
        /// </summary>
        public YarnNode Host { get; set; }

        /// <summary>
        /// <see cref="YarnAppAttempt"/> running in this container
        /// </summary>
        public YarnAppAttempt AppAttempt { get; set; }

        /// <summary>
        /// Priority of the container
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Exit code of the container
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Diagnostics message for failed containers
        /// </summary>
        public string Diagnostics { get; set; }

        /// <summary>
        /// Amount of needed/allocated Memory in MB
        /// </summary>
        public long AllocatedMemory { get; set; }

        /// <summary>
        /// Amound of needed/allocated VCores
        /// </summary>
        public long AllocatedVcores { get; set; }

        #endregion

        #region IYarnReadable Methods

        /// <summary>
        /// Parser to read
        /// </summary>
        public IHadoopParser Parser { get; set; }

        /// <summary>
        /// Reads the current state from Hadoop
        /// </summary>
        public void ReadStatus()
        {
            var parsed = Parser.ParseContainerDetails(ContainerId);

            if(parsed != null)
            {
                StartTime = parsed.StartTime;
                EndTime = parsed.FinishTime;
                State = parsed.State;
                Host = parsed.Host;
                Priority = parsed.Priority;
                ExitCode = parsed.ExitCode;
                Diagnostics = parsed.Diagnostics;
                AllocatedMemory = parsed.MemoryNeeded;
                AllocatedVcores = parsed.VcoresNeeded;
            }
        }

        /// <summary>
        /// Returns the current status as comma seperated string
        /// </summary>
        /// <returns>The status as string</returns>
        public string StatusAsString()
        {
            var type = GetType();
            var properties = new List<PropertyInfo>(type.GetProperties());
            var status = String.Empty;
            foreach(var p in properties)
            {
                var value = String.Empty;
                if(p.PropertyType.IsValueType || p.PropertyType.Name == typeof(String).Name)
                    value = p.GetValue(this)?.ToString();
                else if(typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                {
                    var propVal = p.GetValue(this) as IEnumerable;
                    if(propVal != null)
                    {
                        var cnt = 0;
                        foreach(var unused in propVal) cnt++;
                        value = cnt.ToString();
                    }
                }
                else
                    value = p.GetValue(this)?.GetType().Name;
                status += $"{p.Name}={value},";
            }

            return status;
        }

        #endregion

        #region Methods

        public override void Update()
        {
            if(!String.IsNullOrWhiteSpace(ContainerId))
                ReadStatus();
        }

        #endregion
    }
}