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
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    public static class ParserUtilities
    {

        /// <summary>
        /// Parses the <see cref="YarnNode"/> or returns null if node not found
        /// </summary>
        /// <param name="node">The node id or http-url</param>
        /// <param name="model">The model</param>
        /// <returns>The parsed <see cref="YarnNode"/></returns>
        public static YarnNode ParseNode(string node, Model model)
        {
            var nodeName = Regex.Match(node, @"(https?:\/\/)?([^\:]+)(:\d*)?").Groups[2].Value;
            if(!model.Nodes.ContainsKey(nodeName))
                return null;
            return model.Nodes[nodeName];
        }

        /// <summary>
        /// Parses the <see cref="EAppState"/> or returns the default value <see cref="EAppState.None"/>
        /// </summary>
        /// <param name="state">The state to parse</param>
        /// <returns>The parsed <see cref="EAppState"/></returns>
        public static EAppState ParseAppState(string state)
        {
            EAppState parsedState;
            Enum.TryParse(state, true, out parsedState);
            return parsedState;
        }

        /// <summary>
        /// Parses the <see cref="EContainerState"/> or returns the default value <see cref="EContainerState.None"/>
        /// </summary>
        /// <param name="state">The state to parse</param>
        /// <returns>The parsed <see cref="EContainerState"/></returns>
        public static EContainerState ParseContainerState(string state)
        {
            EContainerState parsedState;
            Enum.TryParse(state, true, out parsedState);
            return parsedState;
        }

        /// <summary>
        /// Parses the <see cref="ENodeState"/> or returns the default value <see cref="ENodeState.None"/>
        /// </summary>
        /// <param name="state">The state to parse</param>
        /// <returns>The parsed <see cref="ENodeState"/></returns>
        public static ENodeState ParseNodeState(string state)
        {
            ENodeState parsedState;
            Enum.TryParse(state, true, out parsedState);
            return parsedState;
        }

        /// <summary>
        /// Parses the <see cref="EFinalStatus"/> or returns the default value <see cref="EFinalStatus.None"/>
        /// </summary>
        /// <param name="finalStatus">The state to parse</param>
        /// <returns>The parsed <see cref="EFinalStatus"/></returns>
        public static EFinalStatus ParseFinalStatus(string finalStatus)
        {
            EFinalStatus parsedState;
            Enum.TryParse(finalStatus, true, out parsedState);
            return parsedState;
        }

        /// <summary>
        /// Parses the integer or returns the default value 0
        /// </summary>
        /// <param name="value">The value to parse</param>
        /// <returns>The parsed integer</returns>
        public static int ParseInt(string value)
        {
            int val;
            Int32.TryParse(value, out val);
            return val;
        }

        /// <summary>
        /// Parses an integer with leading or trailing text or returns the default value 0
        /// </summary>
        /// <param name="value">The value to parse</param>
        /// <returns>The progress</returns>
        public static int ParseIntText(string value)
        {
            return ParseInt(Regex.Match(value, @"\d+").Value);
        }

        /// <summary>
        /// Parses the timestamp with the given format to <see cref="DateTime"/>
        /// or returns the default value <see cref="DateTime.MinValue"/>
        /// </summary>
        /// <param name="value">The value to parse</param>
        /// <param name="format">The time format for parsing or null if convert vom Java Time Millisec</param>
        /// <param name="culture">The <see cref="CultureInfo"/> for parsing, default en-US</param>
        /// <returns>The parsed <see cref="DateTime"/></returns>
        public static DateTime ParseJavaTimestamp(string value, string format, CultureInfo culture = null)
        {
            culture = culture ?? new CultureInfo("en-US");
            if(format != null)
            {
                DateTime time;
                DateTime.TryParseExact(value, format, culture, DateTimeStyles.AssumeUniversal, out time);
                return time;
            }

            // Java time if no format is given
            long javaMillis;
            if(!Int64.TryParse(value, out javaMillis) || javaMillis == 0)
                return DateTime.MinValue;
            var javaTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(javaMillis);
            return javaTimeUtc.ToLocalTime();
        }

        /// <summary>
        /// Concatinates the states to a comma seperated string
        /// </summary>
        /// <param name="states">The states</param>
        /// <returns>The comma seperated string</returns>
        public static string GetStateString(EAppState states)
        {
            var appStates = String.Empty; // default return appStates by hadoop
            if(states != EAppState.None)
                appStates = states.ToString().Replace(" ", "");

            return appStates;
        }
    }

    #region Json

    /// <summary>
    /// Utility class to convert java epoch (ms since 1970-01-01 00:00:00) to <see cref="DateTime"/>
    /// </summary>
    /// <remarks>By mpen, https://stackoverflow.com/a/19972214 </remarks>
    public class JsonJavaEpochConverter : DateTimeConverterBase
    {
        private static readonly DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((DateTime)value - _Epoch).TotalMilliseconds.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //if(reader.Value == null || (long)reader.Value == 0)
            //    return DateTime.MinValue;
            //return _Epoch.AddMilliseconds((long)reader.Value);
            return ParserUtilities.ParseJavaTimestamp((string)reader.Value, null);
        }
    }

    #endregion
}