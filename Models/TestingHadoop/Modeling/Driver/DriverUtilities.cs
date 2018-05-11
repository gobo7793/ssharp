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
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SafetySharp.CaseStudies.TestingHadoop.Modeling.HadoopModel;

namespace SafetySharp.CaseStudies.TestingHadoop.Modeling.Driver
{
    public static class DriverUtilities
    {
        #region Fields

        private static readonly Regex _ParseNodeRegex = new Regex(@"(https?:\/\/)?([^\:]+)(:\d*)?");
        private static readonly Regex _DigitRegex = new Regex(@"\d+");

        private static readonly CultureInfo _DefaultDtCulture = new CultureInfo("en-US");

        #endregion

        #region Parsing

        /// <summary>
        /// Parses the <see cref="YarnNode"/> or returns null if node not found
        /// </summary>
        /// <param name="node">The node id or http-url</param>
        /// <param name="model">The model</param>
        /// <returns>The parsed <see cref="YarnNode"/></returns>
        public static YarnNode ParseNode(string node, Model model)
        {
            var nodeName = _ParseNodeRegex.Match(node).Groups[2].Value;
            var nodeObj = model.Nodes.FirstOrDefault(n => n.Name == nodeName);
            return nodeObj;
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
        /// Parses the first integer value in the string or returns the default value 0
        /// </summary>
        /// <param name="value">The value to parse</param>
        /// <returns>The parsed integer</returns>
        public static int ParseInt(string value)
        {
            int val;
            Int32.TryParse(_DigitRegex.Match(value).Value, out val);
            return val;
        }

        /// <summary>
        /// Parses the timestamp with the given format to <see cref="DateTime"/>
        /// or returns the default value <see cref="DateTime.MinValue"/>
        /// </summary>
        /// <param name="javaMillis">The value to parse</param>
        /// <returns>The parsed <see cref="DateTime"/></returns>
        public static DateTime ParseJavaTimestamp(string javaMillis)
        {
            long javaMillisL;
            if(!Int64.TryParse(javaMillis, out javaMillisL))
                return DateTime.MinValue;
            return ParseJavaTimestamp(javaMillisL);
        }

        /// <summary>
        /// Parses the timestamp with the given format to <see cref="DateTime"/>
        /// or returns the default value <see cref="DateTime.MinValue"/>
        /// </summary>
        /// <param name="javaMillis">The value to parse</param>
        /// <returns>The parsed <see cref="DateTime"/></returns>
        public static DateTime ParseJavaTimestamp(long javaMillis)
        {
            if(javaMillis < 1)
                return DateTime.MinValue;
            var javaTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(javaMillis);
            return javaTimeUtc.ToLocalTime();
        }

        /// <summary>
        /// Parses the timestamp with the given format to <see cref="DateTime"/>
        /// or returns the default value <see cref="DateTime.MinValue"/>
        /// </summary>
        /// <param name="value">The value to parse</param>
        /// <param name="format">The time format for parsing</param>
        /// <param name="culture">The <see cref="CultureInfo"/> for parsing, default en-US</param>
        /// <returns>The parsed <see cref="DateTime"/></returns>
        public static DateTime ParseJavaTimestamp(string value, string format, CultureInfo culture = null)
        {
            culture = culture ?? _DefaultDtCulture;
            DateTime time;
            DateTime.TryParseExact(value, format, culture, DateTimeStyles.AssumeUniversal, out time);
            return time;
        }

        /// <summary>
        /// Concatinates the states to a comma seperated string
        /// </summary>
        /// <param name="states">The states</param>
        /// <returns>The comma seperated string</returns>
        public static string ConcatStates(EAppState states)
        {
            var appStates = String.Empty; // default return appStates by hadoop
            if(states != EAppState.None)
                appStates = states.ToString().Replace(" ", "");

            return appStates;
        }

        #endregion

        #region Converting

        /// <summary>
        /// Converts the given ID type to the target type.
        /// If the input id contains not enough data, the base id for the target type will be returned.
        /// </summary>
        /// <param name="inputId">The id to convert</param>
        /// <param name="targetType">The target type</param>
        /// <returns>The converted id</returns>
        /// <exception cref="ArgumentException">If the inputId is no valid source type id</exception>
        /// <remarks>
        /// Examples:
        /// Attempt->Container: application_1517215519416_0010_000001 -> application_1517215519416_0010_01
        /// Container->App: application_1517215519416_0010_01_000015 -> application_1517215519416_0010
        /// </remarks>
        public static string ConvertId(string inputId, EConvertType targetType)
        {
            return ConvertId(inputId, -1, targetType);
        }

        /// <summary>
        /// Converts the given ID type to the target type with the given target id (only the first level under the source).
        /// If the input id contains not enough data or shortTargetId &gt; 0, the base id for the target type will be returned.
        /// </summary>
        /// <param name="inputId">The id to convert</param>
        /// <param name="shortTargetId">The short target id</param>
        /// <param name="targetType">The target type</param>
        /// <returns>The converted id</returns>
        /// <exception cref="ArgumentException">If the inputId is no valid source type id</exception>
        /// <remarks>
        /// Examples:
        /// Attempt->Container: application_1517215519416_0010_000001 -> application_1517215519416_0010_01
        /// Container->App: application_1517215519416_0010_01_000015 -> application_1517215519416_0010
        /// App->Attempt: application_1517215519416_0010 + 2 -> application_1517215519416_0010_000002
        /// </remarks>
        public static string ConvertId(string inputId, string shortTargetId, EConvertType targetType)
        {
            var shortTargetIdInt = ParseInt(shortTargetId);

            return ConvertId(inputId, shortTargetIdInt > 0 ? shortTargetIdInt : 1, targetType);
        }

        /// <summary>
        /// Converts the given ID type to the target type with the given target id (only the first level under the source).
        /// If the input id contains not enough data or shortTargetId &gt; 0, the base id for the target type will be returned.
        /// </summary>
        /// <param name="inputId">The id to convert</param>
        /// <param name="shortTargetId">The short target id, not needed from container source</param>
        /// <param name="targetType">The target type</param>
        /// <returns>The converted id</returns>
        /// <exception cref="ArgumentException">If the inputId is no valid source type id</exception>
        /// <remarks>
        /// Examples:
        /// Attempt->Container: application_1517215519416_0010_000001 -> application_1517215519416_0010_01
        /// Container->App: application_1517215519416_0010_01_000015 -> application_1517215519416_0010
        /// App->Attempt: application_1517215519416_0010 + 2 -> application_1517215519416_0010_000002
        /// </remarks>
        public static string ConvertId(string inputId, int shortTargetId, EConvertType targetType)
        {
            if(inputId.StartsWith("application"))
                return ConvertAppId(inputId, shortTargetId, targetType);
            if(inputId.StartsWith("appattempt"))
                return ConvertAttemptId(inputId, shortTargetId, targetType);
            if(inputId.StartsWith("container"))
                return ConvertContainerId(inputId, targetType);
            throw new ArgumentException($"{inputId} cannot be convert to {targetType}: source type not supported");
        }

        private static string ConvertAppId(string inputId, int shortId, EConvertType targetType)
        {
            var matches = _DigitRegex.Matches(inputId);
            if(matches.Count != 2)
                throw new ArgumentException($"{inputId} cannot be convert to {targetType}: invalid application id format");

            switch(targetType)
            {
                case EConvertType.Attempt:
                    if(shortId > 0)
                        return $"appattempt_{matches[0].Value}_{matches[1].Value}_{shortId:D6}";
                    else
                        return $"appattempt_{matches[0].Value}_{matches[1].Value}";

                case EConvertType.Container:
                    if(shortId > 0)
                        return $"container_{matches[0].Value}_{matches[1].Value}_{shortId:D2}";
                    else
                        return $"container_{matches[0].Value}_{matches[1].Value}";

                default:
                    return inputId;
            }
        }

        private static string ConvertAttemptId(string inputId, int shortId, EConvertType targetType)
        {
            var matches = _DigitRegex.Matches(inputId);
            if(matches.Count != 3)
                throw new ArgumentException($"{inputId} cannot be convert to {targetType}: invalid appattempt id format");

            var attemptInt = ParseInt(matches[2].Value);

            switch(targetType)
            {
                case EConvertType.App:
                    return $"application_{matches[0].Value}_{matches[1].Value}";

                case EConvertType.Container:
                    if(shortId > 0)
                        return $"container_{matches[0].Value}_{matches[1].Value}_{attemptInt:D2}_{shortId:D6}";
                    else
                        return $"container_{matches[0].Value}_{matches[1].Value}_{attemptInt:D2}";

                default:
                    return inputId;
            }
        }

        private static string ConvertContainerId(string inputId, EConvertType targetType)
        {
            var matches = _DigitRegex.Matches(inputId);
            if(matches.Count != 4)
                throw new ArgumentException($"{inputId} cannot be convert to {targetType}: invalid container id format");

            var attemptInt = ParseInt(matches[2].Value);

            switch(targetType)
            {
                case EConvertType.App:
                    return $"application_{matches[0].Value}_{matches[1].Value}";

                case EConvertType.Attempt:
                    return $"appattempt_{matches[0].Value}_{matches[1].Value}_{attemptInt:D6}";

                default:
                    return inputId;
            }
        }

        #endregion

        #region Multihosting

        /// <summary>
        /// Gets the full node count on all hosts
        /// </summary>
        /// <param name="hostsCount">Count of all hosts</param>
        /// <param name="nodeBaseCount">Base count for nodes (= node count on host 1, on others the half)</param>
        /// <returns>The full node count</returns>
        public static int GetFullNodeCount(int hostsCount, int nodeBaseCount)
        {
            return nodeBaseCount + (hostsCount - 1) * nodeBaseCount / 2;
        }

        /// <summary>
        /// Returns the host id for the given yarn node id (booth one-based index)
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <param name="hostsCount">Count of all hosts</param>
        /// <param name="nodeBaseCount">Base count for nodes (= node count on host 1, on others the half)</param>
        /// <returns>The host id for the node</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If given <paramref name="nodeId"/> is higher than highest possible nodeId based on 
        /// <paramref name="hostsCount"/> and <paramref name="nodeBaseCount"/>,
        /// calculated by <see cref="GetFullNodeCount(int, int)"/>.
        /// </exception>
        public static int GetHostId(int nodeId, int hostsCount, int nodeBaseCount)
        {
            var highestNodeId = GetFullNodeCount(hostsCount, nodeBaseCount);
            if(nodeId > highestNodeId)
                throw new ArgumentOutOfRangeException($"nodeID {nodeId} is out of range, highest nodeId is {highestNodeId}");

            if(nodeId <= nodeBaseCount)
                return 1;
            var hostsNodeId = nodeId - nodeBaseCount;
            var hostsPerNode = nodeBaseCount / 2.0;
            var realHost = hostsNodeId / hostsPerNode;
            var host = Math.Ceiling(realHost);
            var actualHost = (int)host + 1;
            return actualHost;
        }

        #endregion
    }

    #region EConverting

    /// <summary>
    /// Supported converting types
    /// </summary>
    public enum EConvertType
    {
        App,
        Attempt,
        Container
    }

    #endregion

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
            return DriverUtilities.ParseJavaTimestamp((long)reader.Value);
        }
    }

    #endregion
}