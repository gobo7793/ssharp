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

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel
{
    /// <summary>
    /// General utility methods for the hadoop model
    /// </summary>
    public static class ModelUtilities
    {
        /// <summary>
        /// Clears the given char array
        /// </summary>
        /// <param name="array">The char array to clear</param>
        public static void ClearArray(char[] array)
        {
            Array.Clear(array, 0, array.Length);
        }

        /// <summary>
        /// Sets the given char array based on the given string
        /// </summary>
        /// <param name="targetArray">The char array to save in</param>
        /// <param name="sourceString">The source string to save</param>
        public static void SetCharArrayOnString(char[] targetArray, string sourceString)
        {
            ClearArray(targetArray);
            sourceString?.ToCharArray().CopyTo(targetArray, 0);
        }

        /// <summary>
        /// Gets the given char array as string
        /// </summary>
        /// <param name="sourceArray">The array to get</param>
        public static string GetCharArrayAsString(char[] sourceArray)
        {
            return new String(sourceArray).Replace("\0", String.Empty);
        }
    }
}