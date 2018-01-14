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
using SafetySharp.Modeling;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// Base class for all YARN Hosts
    /// </summary>
    [DebuggerDisplay("Name {" + nameof(Name) + "}")]
    public abstract class YarnHost : Component
    {
        private string _HttpUrl;

        protected abstract string HttpPort{ get; }

        /// <summary>
        /// Name of the Host
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// HTTP URL of the Host, requires a <see cref="Name"/>
        /// </summary>
        public string HttpUrl
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_HttpUrl))
                    _HttpUrl = $"http://{Name}:{HttpPort}";
                return _HttpUrl;
            }
        }
    }
}